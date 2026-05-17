using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface IAppMenuRepository : IRepository<AppMenu>
{
    Task<IEnumerable<AppMenu>> GetAllWithSubMenusAsync(CancellationToken ct = default);
    Task<AppSubMenu?> GetSubMenuByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<AppSubMenu>> GetSubMenusByIdsAsync(IEnumerable<Guid> ids,
        CancellationToken ct = default);
}
