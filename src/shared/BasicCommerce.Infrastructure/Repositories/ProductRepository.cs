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

    public async Task<(IEnumerable<ProductListProjection> Items, int TotalCount)> GetPagedProjectedAsync(
        Guid tenantId, int page, int pageSize,
        Guid? categoryId = null, EntityStatus? status = null,
        string? search = null, CancellationToken ct = default)
    {
        var baseQuery =
            from p in Db.Products
            join c in Db.Categories on p.CategoryId equals c.Id
            join vr in Db.VatRates on p.VatRateId equals vr.Id
            join m in Db.Manufacturers on p.ManufacturerId equals m.Id into mj
            from m in mj.DefaultIfEmpty()
            where p.TenantId == tenantId
            select new { p, c, vr, m };

        if (categoryId.HasValue)
            baseQuery = baseQuery.Where(x => x.p.CategoryId == categoryId.Value);

        if (status.HasValue)
            baseQuery = baseQuery.Where(x => x.p.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            baseQuery = baseQuery.Where(x =>
                x.p.Name.ToLower().Contains(term) ||
                x.p.NameBn.Contains(term) ||
                x.p.Sku.ToLower().Contains(term) ||
                x.p.Barcode.Contains(term) ||
                (x.p.Plu != null && x.p.Plu.Contains(term)));
        }

        var total = await baseQuery.CountAsync(ct);

        var rawItems = await baseQuery
            .OrderBy(x => x.p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.p.Id,
                x.p.Sku,
                x.p.Name,
                x.p.NameBn,
                CategoryName = x.c.Name,
                PriceAmount  = x.p.Price.Amount,
                Currency     = x.p.Price.Currency,
                VatRate      = x.vr.Rate,
                x.p.ImageUrl,
                ManufacturerName = (string?)x.m.Name,
                x.p.Status,
            })
            .ToListAsync(ct);

        var items = rawItems.Select(x => new ProductListProjection(
            x.Id, x.Sku, x.Name, x.NameBn,
            x.CategoryName, x.PriceAmount, x.Currency,
            x.VatRate, x.ImageUrl, x.ManufacturerName,
            x.Status.ToString()));

        return (items, total);
    }

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
