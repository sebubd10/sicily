using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface IUserRepository : ITenantRepository<User>
{
    Task<User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken ct = default);
    Task<User?> GetByEmailAcrossTenantsAsync(string email, CancellationToken ct = default);
    Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken ct = default);
    Task<User?> GetByMicrosoftIdAsync(string microsoftId, CancellationToken ct = default);
    Task<User?> GetByEmployeeCodeAsync(Guid tenantId, string employeeCode, CancellationToken ct = default);
}

public interface IUserRefreshTokenRepository : IRepository<UserRefreshToken>
{
    Task<UserRefreshToken?> GetActiveTokenAsync(string token, CancellationToken ct = default);
    Task RevokeAllForUserAsync(Guid userId, string? revokedByIp = null, CancellationToken ct = default);
}
