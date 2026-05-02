using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Auth;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace BasicCommerce.Application.Features.Auth.Commands;

public record ExternalLoginCommand(
    string Provider,
    string ExternalId,
    string Email,
    string FirstName,
    string LastName,
    string? TenantSlug) : IRequest<AuthResponse>;

public class ExternalLoginCommandValidator : AbstractValidator<ExternalLoginCommand>
{
    public ExternalLoginCommandValidator()
    {
        RuleFor(x => x.Provider).NotEmpty()
            .Must(p => p == "Google" || p == "Microsoft")
            .WithMessage("Provider must be 'Google' or 'Microsoft'.");
        RuleFor(x => x.ExternalId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty();
    }
}

public class ExternalLoginCommandHandler : IRequestHandler<ExternalLoginCommand, AuthResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;
    private readonly IConfiguration _config;

    public ExternalLoginCommandHandler(IUnitOfWork uow, IJwtService jwt, IConfiguration config)
    {
        _uow = uow;
        _jwt = jwt;
        _config = config;
    }

    public async Task<AuthResponse> Handle(ExternalLoginCommand request, CancellationToken ct)
    {
        var provider = Enum.Parse<AuthProvider>(request.Provider);

        // Look up existing user by external ID first, then fall back to email
        var user = provider == AuthProvider.Google
            ? await _uow.Users.GetByGoogleIdAsync(request.ExternalId, ct)
            : await _uow.Users.GetByMicrosoftIdAsync(request.ExternalId, ct);

        if (user is null)
        {
            // Check if an account with this email already exists
            user = await _uow.Users.GetByEmailAcrossTenantsAsync(request.Email, ct);

            if (user is not null)
                throw new UnauthorizedException(
                    $"An account with this email already exists. " +
                    $"Please log in with your original method.");

            // Auto-register new customer via social login
            // TenantId is a placeholder — resolved via TenantSlug in a real setup
            var tenantId = Guid.Empty; // TODO: resolve from TenantSlug
            user = User.CreateWithExternalProvider(
                tenantId, request.FirstName, request.LastName,
                request.Email, provider, request.ExternalId);

            await _uow.Users.AddAsync(user, ct);
        }

        if (!user.IsActive)
            throw new UnauthorizedException("Account is deactivated.");

        user.RecordLogin();
        _uow.Users.Update(user);

        var accessToken = _jwt.GenerateAccessToken(user);
        var refreshToken = _jwt.GenerateRefreshToken();
        var expiryDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7");

        await _uow.UserRefreshTokens.AddAsync(
            UserRefreshToken.Create(user.Id, refreshToken, expiryDays), ct);

        await _uow.SaveChangesAsync(ct);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(
                double.Parse(_config["Jwt:ExpiryMinutes"] ?? "60")),
            TokenType: "Bearer",
            User: new UserDto(
                user.Id, user.FullName, user.Email,
                user.Role.ToString(), user.TenantId, user.StoreId,
                user.AuthProvider.ToString(), user.PreferredLanguage));
    }
}
