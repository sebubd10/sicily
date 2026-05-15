using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class ProductImageRepository : TenantRepository<ProductImage>, IProductImageRepository
{
    public ProductImageRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<IEnumerable<ProductImage>> GetByProductAsync(
        Guid tenantId, Guid productId, CancellationToken ct = default) =>
        await Db.ProductImages
            .Where(p => p.TenantId == tenantId && p.ProductId == productId)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);
}
