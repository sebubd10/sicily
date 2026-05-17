using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface IApiPermissionRepository : IRepository<ApiPermission>
{
    Task<ApiPermission?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IEnumerable<ApiPermission>> GetByCodesAsync(IEnumerable<string> codes,
        CancellationToken ct = default);
    Task<IEnumerable<ApiPermission>> GetByGroupAsync(string group,
        CancellationToken ct = default);
    Task<IEnumerable<ApiPermission>> GetAllAsync(CancellationToken ct = default);
}
