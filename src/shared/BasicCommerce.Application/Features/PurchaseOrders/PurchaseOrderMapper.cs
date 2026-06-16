using BasicCommerce.Contracts.PurchaseOrders;
using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Application.Features.PurchaseOrders;

internal static class PurchaseOrderMapper
{
    internal static PurchaseOrderResponse ToResponse(PurchaseOrder po) =>
        new(po.Id, po.OrderNumber,
            po.SupplierId, po.Supplier?.Name ?? string.Empty,
            po.WarehouseId, po.Warehouse?.Name ?? string.Empty,
            po.PurchaseOrderStatus.ToString(),
            po.OrderDate, po.ExpectedDate, po.ReceivedDate,
            po.Notes, po.Currency, po.TotalAmount,
            po.Items.Select(ToItemResponse));

    internal static PurchaseOrderSummaryResponse ToSummary(PurchaseOrder po) =>
        new(po.Id, po.OrderNumber,
            po.SupplierId, po.Supplier?.Name ?? string.Empty,
            po.WarehouseId, po.Warehouse?.Name ?? string.Empty,
            po.PurchaseOrderStatus.ToString(),
            po.OrderDate, po.ExpectedDate, po.Currency, po.TotalAmount,
            po.Items.Count);

    internal static PurchaseOrderItemResponse ToItemResponse(PurchaseOrderItem i) =>
        new(i.Id, i.ProductId, i.Product?.Name ?? string.Empty, i.Product?.Sku ?? string.Empty,
            i.OrderedQuantity, i.ReceivedQuantity, i.RemainingQuantity,
            i.UnitCost, i.TotalCost, i.IsFullyReceived);
}
