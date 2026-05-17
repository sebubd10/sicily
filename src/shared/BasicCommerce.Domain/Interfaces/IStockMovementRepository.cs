using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface IStockMovementRepository : ITenantRepository<StockMovement>
{
    Task<IEnumerable<StockMovement>> GetByProductAsync(Guid tenantId, Guid storeId,
        Guid productId, int limit = 50, CancellationToken ct = default);
    Task<IEnumerable<StockMovement>> GetByStoreAsync(Guid tenantId, Guid storeId,
        DateTime? from = null, DateTime? to = null, int limit = 200, CancellationToken ct = default);
}
