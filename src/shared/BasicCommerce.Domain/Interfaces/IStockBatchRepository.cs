using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface IStockBatchRepository : ITenantRepository<StockBatch>
{
    Task<IEnumerable<StockBatch>> GetActiveBatchesFEFOAsync(
        Guid tenantId, Guid storeId, Guid productId, CancellationToken ct = default);

    Task<IEnumerable<StockBatch>> GetPagedAsync(
        Guid tenantId, Guid storeId, Guid? productId,
        bool includeExpired, int page, int pageSize, CancellationToken ct = default);

    Task<int> GetTotalCountAsync(
        Guid tenantId, Guid storeId, Guid? productId,
        bool includeExpired, CancellationToken ct = default);

    Task<IEnumerable<StockBatch>> GetExpiringAsync(
        Guid tenantId, Guid storeId, int withinDays, CancellationToken ct = default);

    Task<IEnumerable<StockBatch>> GetExpiredUnprocessedAsync(
        Guid tenantId, CancellationToken ct = default);
}
