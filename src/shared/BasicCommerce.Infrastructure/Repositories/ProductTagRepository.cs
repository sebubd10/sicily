using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class ProductTagRepository : TenantRepository<ProductTag>, IProductTagRepository
{
    public ProductTagRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<ProductTag?> GetByNameAsync(Guid tenantId, string name,
        CancellationToken ct = default) =>
        await Db.ProductTags.FirstOrDefaultAsync(
            t => t.TenantId == tenantId && t.Name == name, ct);

    public async Task<IEnumerable<ProductTag>> GetByIdsAsync(Guid tenantId, IEnumerable<Guid> ids,
        CancellationToken ct = default)
    {
        var idList = ids.ToList();
        return await Db.ProductTags
            .Where(t => t.TenantId == tenantId && idList.Contains(t.Id))
            .ToListAsync(ct);
    }

    public async Task<(IEnumerable<(ProductTag Tag, int TaggedProductsCount)> Items, int TotalCount)>
        GetPagedAsync(Guid tenantId, int page, int pageSize, string? search = null,
            CancellationToken ct = default)
    {
        var query = Db.ProductTags.Where(t => t.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Name.Contains(search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new { Tag = t, Count = t.Products.Count() })
            .ToListAsync(ct);

        return (items.Select(x => (x.Tag, x.Count)), total);
    }
}
