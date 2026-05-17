using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class UserTypeRepository : TenantRepository<UserType>, IUserTypeRepository
{
    public UserTypeRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<UserType?> GetWithAccessAsync(Guid tenantId, Guid id,
        CancellationToken ct = default)
        => await Set
            .Include(ut => ut.MenuAccess)
            .Include(ut => ut.Permissions)
            .FirstOrDefaultAsync(ut => ut.TenantId == tenantId && ut.Id == id, ct);

    public async Task<IEnumerable<UserType>> GetAllWithAccessAsync(Guid tenantId,
        CancellationToken ct = default)
        => await Set
            .Include(ut => ut.MenuAccess)
            .Include(ut => ut.Permissions)
            .Where(ut => ut.TenantId == tenantId)
            .ToListAsync(ct);

    public async Task<bool> NameExistsAsync(Guid tenantId, string name, Guid? excludeId = null,
        CancellationToken ct = default)
        => await Set.AnyAsync(ut =>
            ut.TenantId == tenantId
            && ut.Name.ToLower() == name.ToLower()
            && (excludeId == null || ut.Id != excludeId), ct);
}
