using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface ISupplierProductRepository : ITenantRepository<SupplierProduct>
{
    Task<SupplierProduct?> GetBySupplierAndProductAsync(Guid tenantId, Guid supplierId,
        Guid productId, CancellationToken ct = default);
    Task<IEnumerable<SupplierProduct>> GetBySupplierPagedAsync(Guid tenantId, Guid supplierId,
        int page, int pageSize, CancellationToken ct = default);
    Task<int> GetCountBySupplierAsync(Guid tenantId, Guid supplierId,
        CancellationToken ct = default);
    Task<IEnumerable<SupplierProduct>> GetByProductIdAsync(Guid tenantId, Guid productId,
        CancellationToken ct = default);
}
