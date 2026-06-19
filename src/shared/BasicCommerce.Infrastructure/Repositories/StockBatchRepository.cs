using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class StockBatchRepository : TenantRepository<StockBatch>, IStockBatchRepository
{
    public StockBatchRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<IEnumerable<StockBatch>> GetActiveBatchesFEFOAsync(
        Guid tenantId, Guid storeId, Guid productId, CancellationToken ct = default) =>
        await Db.StockBatches
            .Where(b => b.TenantId == tenantId
                && b.StoreId == storeId
                && b.ProductId == productId
                && !b.IsExpired
                && b.RemainingQuantity > 0)
            .OrderBy(b => b.ExpiryDate == null ? 1 : 0)
            .ThenBy(b => b.ExpiryDate)
            .ThenBy(b => b.CreatedAt)
            .ToListAsync(ct);

    public async Task<IEnumerable<StockBatch>> GetPagedAsync(
        Guid tenantId, Guid storeId, Guid? productId,
        bool includeExpired, int page, int pageSize, CancellationToken ct = default) =>
        await Db.StockBatches
            .Include(b => b.Product)
            .Where(b => b.TenantId == tenantId
                && b.StoreId == storeId
                && (productId == null || b.ProductId == productId)
                && (includeExpired || !b.IsExpired))
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<int> GetTotalCountAsync(
        Guid tenantId, Guid storeId, Guid? productId,
        bool includeExpired, CancellationToken ct = default) =>
        await Db.StockBatches
            .CountAsync(b => b.TenantId == tenantId
                && b.StoreId == storeId
                && (productId == null || b.ProductId == productId)
                && (includeExpired || !b.IsExpired), ct);

    public async Task<IEnumerable<StockBatch>> GetExpiringAsync(
        Guid tenantId, Guid storeId, int withinDays, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(withinDays);
        return await Db.StockBatches
            .Include(b => b.Product)
            .Where(b => b.TenantId == tenantId
                && b.StoreId == storeId
                && !b.IsExpired
                && b.RemainingQuantity > 0
                && b.ExpiryDate.HasValue
                && b.ExpiryDate <= cutoff)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<StockBatch>> GetExpiredUnprocessedAsync(
        Guid tenantId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await Db.StockBatches
            .Include(b => b.Product)
            .Where(b => b.TenantId == tenantId
                && !b.IsExpired
                && b.RemainingQuantity > 0
                && b.ExpiryDate.HasValue
                && b.ExpiryDate < now)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync(ct);
    }

    public async Task<bool> HasActiveBatchesForProductAsync(Guid tenantId, Guid productId,
        CancellationToken ct = default) =>
        await Db.StockBatches.AnyAsync(
            b => b.TenantId == tenantId
                && b.ProductId == productId
                && !b.IsExpired
                && b.RemainingQuantity > 0, ct);

    public async Task<bool> HasActiveBatchesForStoreAsync(Guid tenantId, Guid storeId,
        CancellationToken ct = default) =>
        await Db.StockBatches.AnyAsync(
            b => b.TenantId == tenantId
                && b.StoreId == storeId
                && !b.IsExpired
                && b.RemainingQuantity > 0, ct);
}
