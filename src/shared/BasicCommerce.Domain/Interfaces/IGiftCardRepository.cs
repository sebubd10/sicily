using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface IGiftCardRepository : ITenantRepository<GiftCard>
{
    Task<GiftCard?> GetByCodeAsync(Guid tenantId, string code, CancellationToken ct = default);
    Task<GiftCard?> GetWithTransactionsAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<IEnumerable<GiftCard>> GetPagedAsync(Guid tenantId, Guid? storeId, string? status,
        int page, int pageSize, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(Guid tenantId, Guid? storeId, string? status,
        CancellationToken ct = default);
    Task<IEnumerable<GiftCard>> GetExpiredUnprocessedAsync(Guid tenantId,
        CancellationToken ct = default);
}
