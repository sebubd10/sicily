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
    Task<bool> HasPointsForStoreAsync(Guid tenantId, Guid storeId, CancellationToken ct = default);

    /// <summary>Sum of activated earned points on the account whose expiry date has passed.</summary>
    Task<int> GetExpiredEarnedTotalAsync(Guid accountId, DateTime asOf, CancellationToken ct = default);

    /// <summary>All ledger entries recorded against a sale transaction.</summary>
    Task<IReadOnlyList<RewardPointsEntry>> GetEntriesForTransactionAsync(Guid transactionId,
        CancellationToken ct = default);
}
