using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class ApiPermissionRepository : GenericRepository<ApiPermission>, IApiPermissionRepository
{
    public ApiPermissionRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<ApiPermission?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await Set.FirstOrDefaultAsync(p => p.Code == code.ToLowerInvariant(), ct);

    public async Task<IEnumerable<ApiPermission>> GetByCodesAsync(
        IEnumerable<string> codes, CancellationToken ct = default)
    {
        var lower = codes.Select(c => c.ToLowerInvariant()).ToList();
        return await Set.Where(p => lower.Contains(p.Code)).ToListAsync(ct);
    }

    public async Task<IEnumerable<ApiPermission>> GetByGroupAsync(
        string group, CancellationToken ct = default)
        => await Set.Where(p => p.Group == group).OrderBy(p => p.Name).ToListAsync(ct);

    public new async Task<IEnumerable<ApiPermission>> GetAllAsync(
        CancellationToken ct = default)
        => await Set.OrderBy(p => p.Group).ThenBy(p => p.Name).ToListAsync(ct);
}
