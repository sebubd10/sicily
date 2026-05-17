using BasicCommerce.Application.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BasicCommerce.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly BasicCommerceDbContext _db;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public PermissionService(BasicCommerceDbContext db, IMemoryCache cache)
    { _db = db; _cache = cache; }

    public async Task<bool> HasPermissionAsync(Guid? userTypeId, string permissionCode,
        CancellationToken ct = default)
    {
        if (!userTypeId.HasValue) return false;
        var perms = await GetPermissionsAsync(userTypeId.Value, ct);
        return perms.Contains(permissionCode.ToLowerInvariant());
    }

    public async Task<IReadOnlySet<string>> GetPermissionsAsync(
        Guid userTypeId, CancellationToken ct = default)
    {
        var key = $"perms:{userTypeId}";
        if (_cache.TryGetValue(key, out IReadOnlySet<string>? cached) && cached is not null)
            return cached;

        var codes = await _db.Set<Domain.Entities.UserType>()
            .Where(ut => ut.Id == userTypeId)
            .SelectMany(ut => ut.Permissions)
            .Select(p => p.Code)
            .ToListAsync(ct);

        var result = (IReadOnlySet<string>)codes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        _cache.Set(key, result, CacheDuration);
        return result;
    }

    public void InvalidateCache(Guid userTypeId)
        => _cache.Remove($"perms:{userTypeId}");
}
