using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class WarehouseRepository : TenantRepository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<IEnumerable<Warehouse>> GetAllForTenantAsync(Guid tenantId,
        CancellationToken ct = default) =>
        await Db.Warehouses
            .Where(w => w.TenantId == tenantId)
            .OrderBy(w => w.Name)
            .ToListAsync(ct);

    public async Task<Warehouse?> GetDefaultAsync(Guid tenantId, CancellationToken ct = default) =>
        await Db.Warehouses.FirstOrDefaultAsync(
            w => w.TenantId == tenantId && w.IsDefault, ct);
}

public class WarehouseStockLevelRepository : TenantRepository<WarehouseStockLevel>, IWarehouseStockLevelRepository
{
    public WarehouseStockLevelRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<WarehouseStockLevel?> GetAsync(Guid tenantId, Guid warehouseId, Guid productId,
        CancellationToken ct = default) =>
        await Db.WarehouseStockLevels
            .Include(w => w.Product)
            .FirstOrDefaultAsync(w =>
                w.TenantId == tenantId && w.WarehouseId == warehouseId && w.ProductId == productId, ct);

    public async Task<IEnumerable<WarehouseStockLevel>> GetByWarehouseAsync(Guid tenantId, Guid warehouseId,
        CancellationToken ct = default) =>
        await Db.WarehouseStockLevels
            .Include(w => w.Product)
            .Where(w => w.TenantId == tenantId && w.WarehouseId == warehouseId)
            .OrderBy(w => w.Product!.Name)
            .ToListAsync(ct);

    public async Task<IEnumerable<WarehouseStockLevel>> GetLowStockAsync(Guid tenantId, Guid warehouseId,
        CancellationToken ct = default) =>
        await Db.WarehouseStockLevels
            .Include(w => w.Product)
            .Where(w => w.TenantId == tenantId && w.WarehouseId == warehouseId &&
                w.Quantity - w.ReservedQuantity <= w.LowStockThreshold)
            .ToListAsync(ct);
}

public class WarehouseMovementRepository : TenantRepository<WarehouseMovement>, IWarehouseMovementRepository
{
    public WarehouseMovementRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<IEnumerable<WarehouseMovement>> GetByWarehouseAsync(Guid tenantId, Guid warehouseId,
        DateTime? from = null, DateTime? to = null, int limit = 200, CancellationToken ct = default) =>
        await Db.WarehouseMovements
            .Include(m => m.Product)
            .Where(m => m.TenantId == tenantId && m.WarehouseId == warehouseId &&
                (from == null || m.CreatedAt >= from) &&
                (to == null || m.CreatedAt <= to))
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task<IEnumerable<WarehouseMovement>> GetByProductAsync(Guid tenantId, Guid warehouseId,
        Guid productId, int limit = 50, CancellationToken ct = default) =>
        await Db.WarehouseMovements
            .Include(m => m.Product)
            .Where(m => m.TenantId == tenantId && m.WarehouseId == warehouseId && m.ProductId == productId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
}
