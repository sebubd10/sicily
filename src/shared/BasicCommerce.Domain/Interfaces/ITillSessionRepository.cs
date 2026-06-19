using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface ITillSessionRepository : ITenantRepository<TillSession>
{
    Task<TillSession?> GetWithPettyAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<TillSession?> GetOpenSessionAsync(Guid tenantId, Guid terminalId, CancellationToken ct = default);
    Task<IEnumerable<TillSession>> GetPagedAsync(Guid tenantId, Guid? storeId, Guid? terminalId,
        bool openOnly, int page, int pageSize, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(Guid tenantId, Guid? storeId, Guid? terminalId,
        bool openOnly, CancellationToken ct = default);
    Task<bool> HasOpenSessionsForStoreAsync(Guid tenantId, Guid storeId, CancellationToken ct = default);
}
