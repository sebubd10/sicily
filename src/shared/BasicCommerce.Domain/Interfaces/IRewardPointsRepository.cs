using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface IRewardPointsSettingsRepository : ITenantRepository<RewardPointsSettings>
{
    Task<RewardPointsSettings?> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
}

public interface IRewardPointsAccountRepository : ITenantRepository<RewardPointsAccount>
{
    Task<RewardPointsAccount?> GetByCustomerAsync(Guid tenantId, Guid customerId,
        Guid? storeId = null, CancellationToken ct = default);
    Task<RewardPointsAccount?> GetWithEntriesAsync(Guid tenantId, Guid customerId,
        Guid? storeId = null, CancellationToken ct = default);
}
