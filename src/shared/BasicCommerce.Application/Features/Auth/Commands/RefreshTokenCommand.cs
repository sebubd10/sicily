using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Auth;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace BasicCommerce.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken, string? IpAddress = null)
    : IRequest<AuthResponse>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtService _jwt;
    private readonly IConfiguration _config;

    public RefreshTokenCommandHandler(IUnitOfWork uow, IJwtService jwt, IConfiguration config)
    {
        _uow = uow;
        _jwt = jwt;
        _config = config;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var existing = await _uow.UserRefreshTokens.GetActiveTokenAsync(request.RefreshToken, ct)
            ?? throw new UnauthorizedException("Invalid or expired refresh token.");

        var user = await _uow.Users.GetByIdAsync(existing.UserId, ct)
            ?? throw new UnauthorizedException("User not found.");

        if (!user.IsActive)
            throw new UnauthorizedException("Account is deactivated.");

        // Rotate refresh token
        existing.Revoke(request.IpAddress);
        _uow.UserRefreshTokens.Update(existing);

        var newRefreshToken = _jwt.GenerateRefreshToken();
        var expiryDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7");
        await _uow.UserRefreshTokens.AddAsync(
            UserRefreshToken.Create(user.Id, newRefreshToken, expiryDays, request.IpAddress), ct);

        var accessToken = _jwt.GenerateAccessToken(user);
        await _uow.SaveChangesAsync(ct);

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: newRefreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(
                double.Parse(_config["Jwt:ExpiryMinutes"] ?? "60")),
            TokenType: "Bearer",
            User: new UserDto(
                user.Id, user.FullName, user.Email,
                user.Role.ToString(), user.TenantId, user.StoreId,
                user.AuthProvider.ToString(), user.PreferredLanguage));
    }
}
