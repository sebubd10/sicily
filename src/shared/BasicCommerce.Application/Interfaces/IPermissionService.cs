namespace BasicCommerce.Application.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid? userTypeId, string permissionCode,
        CancellationToken ct = default);
    Task<IReadOnlySet<string>> GetPermissionsAsync(Guid userTypeId,
        CancellationToken ct = default);
    void InvalidateCache(Guid userTypeId);
}
