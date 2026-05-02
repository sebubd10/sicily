using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class ProductRepository : TenantRepository<Product>, IProductRepository
{
    public ProductRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<Product?> GetByBarcodeAsync(Guid tenantId, string barcode,
        CancellationToken ct = default) =>
        await Db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(
                p => p.TenantId == tenantId && p.Barcode == barcode && p.IsActive, ct);

    public async Task<Product?> GetByPluAsync(Guid tenantId, string plu,
        CancellationToken ct = default) =>
        await Db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(
                p => p.TenantId == tenantId && p.Plu == plu && p.IsActive, ct);

    public async Task<Product?> GetBySkuAsync(Guid tenantId, string sku,
        CancellationToken ct = default) =>
        await Db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(
                p => p.TenantId == tenantId && p.Sku == sku && p.IsActive, ct);

    public async Task<IEnumerable<Product>> SearchAsync(Guid tenantId, string term,
        int limit = 20, CancellationToken ct = default) =>
        await Db.Products
            .Where(p => p.TenantId == tenantId && p.IsActive &&
                (p.Name.Contains(term) || p.NameBn.Contains(term) ||
                 p.Sku.Contains(term) || p.Barcode.Contains(term)))
            .Take(limit)
            .ToListAsync(ct);
}

public class StockLevelRepository : TenantRepository<StockLevel>, IStockLevelRepository
{
    public StockLevelRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<StockLevel?> GetAsync(Guid tenantId, Guid storeId, Guid productId,
        CancellationToken ct = default) =>
        await Db.StockLevels.FirstOrDefaultAsync(
            s => s.TenantId == tenantId && s.StoreId == storeId && s.ProductId == productId, ct);

    public async Task<IEnumerable<StockLevel>> GetLowStockAsync(Guid tenantId, Guid storeId,
        CancellationToken ct = default) =>
        await Db.StockLevels
            .Include(s => s.Product)
            .Where(s => s.TenantId == tenantId && s.StoreId == storeId &&
                s.Quantity <= s.LowStockThreshold)
            .ToListAsync(ct);
}
