using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class StockMovementRepository : TenantRepository<StockMovement>, IStockMovementRepository
{
    public StockMovementRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<IEnumerable<StockMovement>> GetByProductAsync(Guid tenantId, Guid storeId,
        Guid productId, int limit = 50, CancellationToken ct = default) =>
        await Db.StockMovements
            .Include(m => m.Product)
            .Where(m => m.TenantId == tenantId && m.StoreId == storeId && m.ProductId == productId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);

    public async Task<IEnumerable<StockMovement>> GetByStoreAsync(Guid tenantId, Guid storeId,
        DateTime? from = null, DateTime? to = null, int limit = 200, CancellationToken ct = default) =>
        await Db.StockMovements
            .Include(m => m.Product)
            .Where(m => m.TenantId == tenantId && m.StoreId == storeId &&
                (from == null || m.CreatedAt >= from) &&
                (to == null || m.CreatedAt <= to))
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
}
