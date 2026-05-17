using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface IProductTagRepository : ITenantRepository<ProductTag>
{
    Task<ProductTag?> GetByNameAsync(Guid tenantId, string name, CancellationToken ct = default);
    Task<IEnumerable<ProductTag>> GetByIdsAsync(Guid tenantId, IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<(IEnumerable<(ProductTag Tag, int TaggedProductsCount)> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId, int page, int pageSize, string? search = null, CancellationToken ct = default);
}
