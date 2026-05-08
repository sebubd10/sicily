using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class AppMenuRepository : GenericRepository<AppMenu>, IAppMenuRepository
{
    private readonly BasicCommerceDbContext _db;

    public AppMenuRepository(BasicCommerceDbContext db) : base(db) => _db = db;

    public async Task<IEnumerable<AppMenu>> GetAllWithSubMenusAsync(
        CancellationToken ct = default)
        => await Set.Include(m => m.SubMenus)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(ct);

    public async Task<AppSubMenu?> GetSubMenuByIdAsync(Guid id,
        CancellationToken ct = default)
        => await _db.Set<AppSubMenu>().FindAsync([id], ct);

    public async Task<IEnumerable<AppSubMenu>> GetSubMenusByIdsAsync(
        IEnumerable<Guid> ids, CancellationToken ct = default)
        => await _db.Set<AppSubMenu>()
            .Where(sm => ids.Contains(sm.Id))
            .ToListAsync(ct);
}
