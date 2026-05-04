using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface IManufacturerRepository : ITenantRepository<Manufacturer>
{
    Task<IEnumerable<Manufacturer>> GetAllForTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid tenantId, string name, Guid? excludeId = null, CancellationToken ct = default);
}
