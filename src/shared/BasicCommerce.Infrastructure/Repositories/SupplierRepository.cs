using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class SupplierRepository : TenantRepository<Supplier>, ISupplierRepository
{
    public SupplierRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<IEnumerable<Supplier>> GetAllForTenantAsync(Guid tenantId,
        CancellationToken ct = default) =>
        await Db.Suppliers
            .Include(s => s.Manufacturer)
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(Guid tenantId, string code, Guid? excludeId = null,
        CancellationToken ct = default) =>
        await Db.Suppliers.AnyAsync(s =>
            s.TenantId == tenantId && s.Code == code &&
            (excludeId == null || s.Id != excludeId), ct);
}
