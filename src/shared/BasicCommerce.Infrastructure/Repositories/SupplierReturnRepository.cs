using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class SupplierReturnRepository : TenantRepository<SupplierReturn>, ISupplierReturnRepository
{
    public SupplierReturnRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<SupplierReturn?> GetWithItemsAsync(
        Guid tenantId, Guid id, CancellationToken ct = default) =>
        await Db.SupplierReturns
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .Include(r => r.Supplier)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id, ct);

    public async Task<IEnumerable<SupplierReturn>> GetPagedAsync(
        Guid tenantId, Guid? supplierId, Guid? storeId,
        SupplierReturnStatus? status, int page, int pageSize, CancellationToken ct = default) =>
        await Db.SupplierReturns
            .Include(r => r.Supplier)
            .Where(r => r.TenantId == tenantId
                && (supplierId == null || r.SupplierId == supplierId)
                && (storeId == null || r.StoreId == storeId)
                && (status == null || r.ReturnStatus == status))
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<int> GetTotalCountAsync(
        Guid tenantId, Guid? supplierId, Guid? storeId,
        SupplierReturnStatus? status, CancellationToken ct = default) =>
        await Db.SupplierReturns
            .CountAsync(r => r.TenantId == tenantId
                && (supplierId == null || r.SupplierId == supplierId)
                && (storeId == null || r.StoreId == storeId)
                && (status == null || r.ReturnStatus == status), ct);
}
