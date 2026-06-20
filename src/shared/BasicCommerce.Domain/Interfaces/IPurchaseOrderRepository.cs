using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Interfaces;

public interface IPurchaseOrderRepository : ITenantRepository<PurchaseOrder>
{
    Task<PurchaseOrder?> GetWithItemsAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<(IEnumerable<PurchaseOrder> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId, int page, int pageSize,
        Guid? supplierId = null, Guid? warehouseId = null,
        PurchaseOrderStatus? status = null,
        DateTime? from = null, DateTime? to = null,
        string? dateField = null,
        CancellationToken ct = default);
    Task<bool> OrderNumberExistsAsync(Guid tenantId, string orderNumber, CancellationToken ct = default);
    Task<bool> HasOpenOrdersForProductAsync(Guid tenantId, Guid productId, CancellationToken ct = default);
    Task<bool> HasOpenOrdersForSupplierAsync(Guid tenantId, Guid supplierId, CancellationToken ct = default);
    void RemoveItems(IEnumerable<PurchaseOrderItem> items);
    void AddItems(IEnumerable<PurchaseOrderItem> items);
}
