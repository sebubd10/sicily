using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class ManufacturerRepository : TenantRepository<Manufacturer>, IManufacturerRepository
{
    public ManufacturerRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<IEnumerable<Manufacturer>> GetAllForTenantAsync(Guid tenantId,
        CancellationToken ct = default) =>
        await Db.Manufacturers
            .Where(m => m.TenantId == tenantId)
            .OrderBy(m => m.Name)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(Guid tenantId, string name, Guid? excludeId = null,
        CancellationToken ct = default) =>
        await Db.Manufacturers.AnyAsync(m =>
            m.TenantId == tenantId && m.Name == name &&
            (excludeId == null || m.Id != excludeId), ct);
}
