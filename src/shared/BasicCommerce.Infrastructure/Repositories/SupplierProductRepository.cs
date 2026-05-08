using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class SupplierProductRepository
    : TenantRepository<SupplierProduct>, ISupplierProductRepository
{
    public SupplierProductRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<SupplierProduct?> GetBySupplierAndProductAsync(
        Guid tenantId, Guid supplierId, Guid productId, CancellationToken ct = default)
        => await Set.FirstOrDefaultAsync(
            sp => sp.TenantId == tenantId
               && sp.SupplierId == supplierId
               && sp.ProductId == productId, ct);

    public async Task<IEnumerable<SupplierProduct>> GetBySupplierPagedAsync(
        Guid tenantId, Guid supplierId, int page, int pageSize, CancellationToken ct = default)
        => await Set
            .Where(sp => sp.TenantId == tenantId && sp.SupplierId == supplierId)
            .OrderBy(sp => sp.ProductId)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

    public async Task<int> GetCountBySupplierAsync(
        Guid tenantId, Guid supplierId, CancellationToken ct = default)
        => await Set.CountAsync(
            sp => sp.TenantId == tenantId && sp.SupplierId == supplierId, ct);

    public async Task<IEnumerable<SupplierProduct>> GetByProductIdAsync(
        Guid tenantId, Guid productId, CancellationToken ct = default)
        => await Set
            .Where(sp => sp.TenantId == tenantId && sp.ProductId == productId)
            .OrderBy(sp => sp.UnitCost)
            .ToListAsync(ct);
}
