namespace BasicCommerce.Contracts.PurchaseOrders;

public record PurchaseOrderResponse(
    Guid Id,
    string OrderNumber,
    Guid SupplierId,
    string SupplierName,
    Guid WarehouseId,
    string WarehouseName,
    string Status,
    DateTime OrderDate,
    DateTime? ExpectedDate,
    DateTime? ReceivedDate,
    string? Notes,
    string Currency,
    decimal TotalAmount,
    IEnumerable<PurchaseOrderItemResponse> Items);

public record PurchaseOrderSummaryResponse(
    Guid Id,
    string OrderNumber,
    Guid SupplierId,
    string SupplierName,
    Guid WarehouseId,
    string WarehouseName,
    string Status,
    DateTime OrderDate,
    DateTime? ExpectedDate,
    string Currency,
    decimal TotalAmount,
    int ItemsCount);

public record PurchaseOrderListResponse(
    IEnumerable<PurchaseOrderSummaryResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record PurchaseOrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal OrderedQuantity,
    decimal ReceivedQuantity,
    decimal RemainingQuantity,
    decimal UnitCost,
    decimal TotalCost,
    bool IsFullyReceived);

public record CreatePurchaseOrderRequest(
    Guid SupplierId,
    Guid WarehouseId,
    DateTime OrderDate,
    DateTime? ExpectedDate = null,
    string? Notes = null,
    string Currency = "BDT",
    IEnumerable<PurchaseOrderItemRequest>? Items = null);

public record PurchaseOrderItemRequest(
    Guid ProductId,
    decimal Quantity,
    decimal? UnitCost = null);

public record UpdatePurchaseOrderRequest(
    Guid SupplierId,
    Guid WarehouseId,
    DateTime OrderDate,
    DateTime? ExpectedDate = null,
    string? Notes = null,
    string Currency = "BDT",
    IEnumerable<PurchaseOrderItemRequest>? Items = null);

public record ReceivePurchaseOrderRequest(
    IEnumerable<ReceiveItemRequest> Items,
    string? Notes = null);

public record ReceiveItemRequest(
    Guid ProductId,
    decimal ReceivedQuantity);
