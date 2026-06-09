using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class UserRepository : TenantRepository<User>, IUserRepository
{
    public UserRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<User?> GetByEmailAsync(Guid tenantId, string email,
        CancellationToken ct = default) =>
        await Db.Users.FirstOrDefaultAsync(
            u => u.TenantId == tenantId && u.Email == email.ToLowerInvariant(), ct);

    public async Task<User?> GetByEmailAcrossTenantsAsync(string email,
        CancellationToken ct = default) =>
        await Db.Users.FirstOrDefaultAsync(
            u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<User?> GetByGoogleIdAsync(string googleId,
        CancellationToken ct = default) =>
        await Db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId, ct);

    public async Task<User?> GetByMicrosoftIdAsync(string microsoftId,
        CancellationToken ct = default) =>
        await Db.Users.FirstOrDefaultAsync(u => u.MicrosoftId == microsoftId, ct);

    public async Task<User?> GetByEmployeeCodeAsync(Guid tenantId, string employeeCode,
        CancellationToken ct = default) =>
        await Db.Users.FirstOrDefaultAsync(
            u => u.TenantId == tenantId && u.EmployeeCode == employeeCode, ct);

    public async Task<int> CountByUserTypeAsync(Guid tenantId, Guid userTypeId,
        CancellationToken ct = default) =>
        await Db.Users.CountAsync(
            u => u.TenantId == tenantId && u.UserTypeId == userTypeId, ct);
}

public class UserRefreshTokenRepository : GenericRepository<UserRefreshToken>,
    IUserRefreshTokenRepository
{
    public UserRefreshTokenRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<UserRefreshToken?> GetActiveTokenAsync(string token,
        CancellationToken ct = default) =>
        await Db.UserRefreshTokens.FirstOrDefaultAsync(
            t => t.Token == token && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow, ct);

    public async Task RevokeAllForUserAsync(Guid userId, string? revokedByIp = null,
        CancellationToken ct = default)
    {
        var tokens = await Db.UserRefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in tokens)
            token.Revoke(revokedByIp);
    }
}
