namespace BasicCommerce.Domain.Interfaces;

public interface IProductImageRepository : ITenantRepository<Entities.ProductImage>
{
    Task<IEnumerable<Entities.ProductImage>> GetByProductAsync(
        Guid tenantId, Guid productId, CancellationToken ct = default);
}
