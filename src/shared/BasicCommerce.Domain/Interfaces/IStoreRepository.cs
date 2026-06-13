using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface IStoreRepository : ITenantRepository<Store>
{
    Task<Store?> GetByCodeAsync(Guid tenantId, string code, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(Guid tenantId, string code, CancellationToken ct = default);
}

public interface ITerminalRepository : ITenantRepository<Terminal>
{
    Task<IEnumerable<Terminal>> GetByStoreAsync(Guid tenantId, Guid storeId, CancellationToken ct = default);
    Task<Terminal?> GetByCodeAsync(Guid tenantId, Guid storeId, string code, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(Guid tenantId, Guid storeId, string code, Guid? excludeId = null, CancellationToken ct = default);
}
