using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class PurchaseOrderRepository : TenantRepository<PurchaseOrder>, IPurchaseOrderRepository
{
    public PurchaseOrderRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<PurchaseOrder?> GetWithItemsAsync(Guid tenantId, Guid id,
        CancellationToken ct = default) =>
        await Db.PurchaseOrders
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .Include(p => p.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id, ct);

    public async Task<(IEnumerable<PurchaseOrder> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId, int page, int pageSize,
        Guid? supplierId = null, Guid? warehouseId = null,
        PurchaseOrderStatus? status = null, CancellationToken ct = default)
    {
        var query = Db.PurchaseOrders
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .Where(p => p.TenantId == tenantId);

        if (supplierId.HasValue)
            query = query.Where(p => p.SupplierId == supplierId.Value);
        if (warehouseId.HasValue)
            query = query.Where(p => p.WarehouseId == warehouseId.Value);
        if (status.HasValue)
            query = query.Where(p => p.PurchaseOrderStatus == status.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<bool> OrderNumberExistsAsync(Guid tenantId, string orderNumber,
        CancellationToken ct = default) =>
        await Db.PurchaseOrders.AnyAsync(
            p => p.TenantId == tenantId && p.OrderNumber == orderNumber, ct);
}
