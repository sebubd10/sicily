using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface ISupplierRepository : ITenantRepository<Supplier>
{
    Task<IEnumerable<Supplier>> GetAllForTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid tenantId, string code, Guid? excludeId = null, CancellationToken ct = default);
}
