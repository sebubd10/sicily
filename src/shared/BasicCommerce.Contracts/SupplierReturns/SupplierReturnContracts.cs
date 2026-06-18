namespace BasicCommerce.Contracts.SupplierReturns;

public record SupplierReturnItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    bool IsProductActive,
    decimal Quantity,
    decimal UnitCost,
    decimal TotalCost,
    string Reason,
    string? Notes);

public record SupplierReturnResponse(
    Guid Id,
    string ReturnNumber,
    Guid SupplierId,
    string SupplierName,
    Guid StoreId,
    string StoreName,
    Guid? PurchaseOrderId,
    string Status,
    string? Notes,
    decimal TotalReturnValue,
    decimal? ExpectedCreditAmount,
    decimal? ActualCreditAmount,
    string? CreditNoteReference,
    DateTime? ShippedAt,
    DateTime? CreditReceivedAt,
    IReadOnlyList<SupplierReturnItemResponse> Items,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record SupplierReturnSummaryResponse(
    Guid Id,
    string ReturnNumber,
    Guid SupplierId,
    string SupplierName,
    Guid StoreId,
    string StoreName,
    string Status,
    decimal TotalReturnValue,
    decimal? ExpectedCreditAmount,
    decimal? ActualCreditAmount,
    DateTime CreatedAt,
    DateTime? ShippedAt);

public record SupplierReturnListResponse(
    IReadOnlyList<SupplierReturnSummaryResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record CreateSupplierReturnRequest(
    Guid SupplierId,
    Guid StoreId,
    Guid? PurchaseOrderId = null,
    string? Notes = null);

public record AddSupplierReturnItemRequest(
    Guid ProductId,
    decimal Quantity,
    decimal UnitCost,
    string Reason = "Damaged",
    string? Notes = null);

public record RemoveSupplierReturnItemRequest(Guid ProductId);

public record SetExpectedCreditRequest(decimal ExpectedCreditAmount);

public record ReceiveCreditRequest(
    decimal CreditAmount,
    string? CreditNoteReference = null);
