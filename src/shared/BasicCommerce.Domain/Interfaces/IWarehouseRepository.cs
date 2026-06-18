using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface IWarehouseRepository : ITenantRepository<Warehouse>
{
    Task<IEnumerable<Warehouse>> GetAllForTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<Warehouse?> GetDefaultAsync(Guid tenantId, CancellationToken ct = default);
}

public interface IWarehouseStockLevelRepository : ITenantRepository<WarehouseStockLevel>
{
    Task<WarehouseStockLevel?> GetAsync(Guid tenantId, Guid warehouseId, Guid productId, CancellationToken ct = default);
    Task<IEnumerable<WarehouseStockLevel>> GetByWarehouseAsync(Guid tenantId, Guid warehouseId, CancellationToken ct = default);
    Task<IEnumerable<WarehouseStockLevel>> GetLowStockAsync(Guid tenantId, Guid warehouseId, CancellationToken ct = default);
    Task<bool> HasStockForProductAsync(Guid tenantId, Guid productId, CancellationToken ct = default);
}

public interface IWarehouseMovementRepository : ITenantRepository<WarehouseMovement>
{
    Task<IEnumerable<WarehouseMovement>> GetByWarehouseAsync(Guid tenantId, Guid warehouseId,
        DateTime? from = null, DateTime? to = null, int limit = 200, CancellationToken ct = default);
    Task<IEnumerable<WarehouseMovement>> GetByProductAsync(Guid tenantId, Guid warehouseId,
        Guid productId, int limit = 50, CancellationToken ct = default);
}
