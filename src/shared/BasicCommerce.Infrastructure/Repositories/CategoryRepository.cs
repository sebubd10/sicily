using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class CategoryRepository : TenantRepository<Category>, ICategoryRepository
{
    public CategoryRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<IEnumerable<Category>> GetRootCategoriesAsync(Guid tenantId,
        CancellationToken ct = default) =>
        await Db.Categories
            .Where(c => c.TenantId == tenantId && c.ParentCategoryId == null)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<IEnumerable<Category>> GetChildrenAsync(Guid tenantId, Guid parentId,
        CancellationToken ct = default) =>
        await Db.Categories
            .Where(c => c.TenantId == tenantId && c.ParentCategoryId == parentId)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<bool> NameExistsAsync(Guid tenantId, string name,
        Guid? excludeId = null, CancellationToken ct = default) =>
        await Db.Categories.AnyAsync(c =>
            c.TenantId == tenantId && c.Name == name &&
            (excludeId == null || c.Id != excludeId), ct);
}
