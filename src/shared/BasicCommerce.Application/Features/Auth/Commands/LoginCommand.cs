using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Auth;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace BasicCommerce.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password, string? TenantSlug) : IRequest<AuthResponse>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtService _jwt;
    private readonly IConfiguration _config;

    public LoginCommandHandler(IUnitOfWork uow, IPasswordHasher hasher,
        IJwtService jwt, IConfiguration config)
    {
        _uow = uow;
        _hasher = hasher;
        _jwt = jwt;
        _config = config;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _uow.Users.GetByEmailAcrossTenantsAsync(request.Email, ct)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedException("Account is deactivated.");

        if (user.IsLocked)
            throw new UnauthorizedException("Account is temporarily locked. Please try again later.");

        if (!_hasher.Verify(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            _uow.Users.Update(user);
            await _uow.SaveChangesAsync(ct);
            throw new UnauthorizedException("Invalid email or password.");
        }

        user.RecordLogin();
        _uow.Users.Update(user);

        var accessToken = _jwt.GenerateAccessToken(user);
        var refreshToken = _jwt.GenerateRefreshToken();
        var expiryDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7");

        var refreshTokenEntity = UserRefreshToken.Create(user.Id, refreshToken, expiryDays);
        await _uow.UserRefreshTokens.AddAsync(refreshTokenEntity, ct);
        await _uow.SaveChangesAsync(ct);

        return BuildResponse(user, accessToken, refreshToken);
    }

    private AuthResponse BuildResponse(Domain.Entities.User user, string accessToken, string refreshToken) =>
        new(
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
