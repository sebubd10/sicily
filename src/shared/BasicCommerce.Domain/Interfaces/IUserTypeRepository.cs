using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface IUserTypeRepository : ITenantRepository<UserType>
{
    Task<UserType?> GetWithAccessAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<IEnumerable<UserType>> GetAllWithAccessAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeId = null,
        CancellationToken ct = default);
}
