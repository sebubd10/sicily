using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface ICategoryRepository : ITenantRepository<Category>
{
    Task<IEnumerable<Category>> GetRootCategoriesAsync(Guid tenantId, CancellationToken ct = default);
    Task<IEnumerable<Category>> GetChildrenAsync(Guid tenantId, Guid parentId, CancellationToken ct = default);
    Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeId = null, CancellationToken ct = default);
    Task<Category?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default);
}
