using BasicCommerce.Contracts.Warehouses;
using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Application.Features.Warehouses;

internal static class WarehouseMapper
{
    internal static WarehouseResponse ToResponse(Warehouse w) =>
        new(w.Id, w.Name, w.Code, w.Phone, w.Email,
            w.Address.Line1, w.Address.City, w.IsDefault, w.Status.ToString());

    internal static WarehouseStockLevelResponse ToStockResponse(WarehouseStockLevel s, string warehouseName) =>
        new(s.WarehouseId, warehouseName, s.ProductId,
            s.Product?.Name ?? string.Empty, s.Product?.Sku ?? string.Empty,
            s.Product?.Barcode ?? string.Empty,
            s.Quantity, s.ReservedQuantity, s.AvailableQuantity,
            s.LowStockThreshold, s.IsLowStock);

    internal static WarehouseMovementResponse ToMovementResponse(WarehouseMovement m) =>
        new(m.Id, m.MovementType.ToString(), m.ProductId,
            m.Product?.Name ?? string.Empty, m.Product?.Sku ?? string.Empty,
            m.Quantity, m.QuantityBefore, m.QuantityAfter,
            m.RelatedStoreId, m.PurchaseOrderId, m.Reference, m.Notes,
            m.RecordedByUserId, m.CreatedAt);
}
