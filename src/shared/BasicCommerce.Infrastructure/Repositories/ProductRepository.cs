using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
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
                p => p.TenantId == tenantId && p.Barcode == barcode && p.Status == EntityStatus.Active, ct);

    public async Task<Product?> GetByNameAsync(Guid tenantId, string name,
        CancellationToken ct = default) =>
        await Db.Products
            .FirstOrDefaultAsync(
                p => p.TenantId == tenantId && p.Name.ToLower() == name.ToLower() && p.Status == EntityStatus.Active, ct);

    public async Task<Product?> GetByPluAsync(Guid tenantId, string plu,
        CancellationToken ct = default) =>
        await Db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(
                p => p.TenantId == tenantId && p.Plu == plu && p.Status == EntityStatus.Active, ct);

    public async Task<Product?> GetBySkuAsync(Guid tenantId, string sku,
        CancellationToken ct = default) =>
        await Db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(
                p => p.TenantId == tenantId && p.Sku == sku && p.Status == EntityStatus.Active, ct);

    public async Task<Product?> GetWithTagsAsync(Guid tenantId, Guid id,
        CancellationToken ct = default) =>
        await Db.Products
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id, ct);

    public async Task<IEnumerable<Product>> SearchAsync(Guid tenantId, string term,
        int limit = 20, CancellationToken ct = default) =>
        await Db.Products
            .Where(p => p.TenantId == tenantId && p.Status == EntityStatus.Active &&
                (p.Name.Contains(term) || p.NameBn.Contains(term) ||
                 p.Sku.Contains(term) || p.Barcode.Contains(term)))
            .Take(limit)
            .ToListAsync(ct);

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId, int page, int pageSize,
        Guid? categoryId = null, EntityStatus? status = null,
        string? search = null, CancellationToken ct = default)
    {
        var query = Db.Products
            .Include(p => p.Category)
            .Include(p => p.Manufacturer)
            .Where(p => p.TenantId == tenantId);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.NameBn.Contains(term) ||
                p.Sku.ToLower().Contains(term) ||
                p.Barcode.Contains(term) ||
                (p.Plu != null && p.Plu.Contains(term)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IEnumerable<Product>> GetAllForTenantAsync(
        Guid tenantId, Guid? categoryId = null, bool includeInactive = false,
        string? search = null, CancellationToken ct = default)
    {
        var query = Db.Products
            .Include(p => p.Category)
            .Include(p => p.Manufacturer)
            .Where(p => p.TenantId == tenantId);

        if (!includeInactive)
            query = query.Where(p => p.Status == EntityStatus.Active);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Sku.ToLower().Contains(term) ||
                p.Barcode.Contains(term));
        }

        return await query.OrderBy(p => p.Name).ToListAsync(ct);
    }
}

public class StockLevelRepository : TenantRepository<StockLevel>, IStockLevelRepository
{
    public StockLevelRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<StockLevel?> GetAsync(Guid tenantId, Guid storeId, Guid productId,
        CancellationToken ct = default) =>
        await Db.StockLevels.FirstOrDefaultAsync(
            s => s.TenantId == tenantId && s.StoreId == storeId && s.ProductId == productId, ct);

    public async Task<IEnumerable<StockLevel>> GetByStoreAsync(Guid tenantId, Guid storeId,
        CancellationToken ct = default) =>
        await Db.StockLevels
            .Include(s => s.Product)
            .Where(s => s.TenantId == tenantId && s.StoreId == storeId)
            .OrderBy(s => s.Product!.Name)
            .ToListAsync(ct);

    public async Task<IEnumerable<StockLevel>> GetLowStockAsync(Guid tenantId, Guid storeId,
        CancellationToken ct = default) =>
        await Db.StockLevels
            .Include(s => s.Product)
            .Where(s => s.TenantId == tenantId && s.StoreId == storeId &&
                s.Quantity <= s.LowStockThreshold)
            .ToListAsync(ct);
}
