namespace BasicCommerce.Contracts.Transactions;

public record TransactionResponse(
    Guid Id,
    string TransactionNumber,
    string Status,
    string Type,
    Guid StoreId,
    Guid TerminalId,
    Guid CashierId,
    Guid? CustomerId,
    string? CustomerName,
    Guid? OriginalTransactionId,
    IEnumerable<LineItemResponse> LineItems,
    IEnumerable<PaymentResponse> Payments,
    decimal SubTotal,
    decimal TaxTotal,
    decimal DiscountTotal,
    decimal Total,
    decimal AmountPaid,
    decimal ChangeDue,
    string? Notes,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    DateTime? VoidedAt,
    string? VoidReason);

public record TransactionSummary(
    Guid Id,
    string TransactionNumber,
    string Status,
    string Type,
    Guid StoreId,
    string? StoreName,
    Guid? CustomerId,
    string? CustomerName,
    decimal Total,
    decimal AmountPaid,
    int ItemCount,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public record TransactionListResponse(
    IEnumerable<TransactionSummary> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record BackofficeVoidRequest(string Reason);

public record BackofficeReturnRequest(
    IEnumerable<ReturnLineItemRequest> Items,
    string RefundMethod,
    string? Notes = null);

public record LineItemResponse(
    Guid Id,
    string ProductName,
    string ProductSku,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal TaxAmount,
    decimal DiscountAmount,
    decimal LineTotal,
    bool IsVoided,
    bool IsPriceOverridden,
    string? ReturnReason,
    string? DamageDisposition,
    Guid? AppliedPromotionId,
    string? AppliedPromotionName);

public record PaymentResponse(
    Guid Id,
    string Method,
    decimal Amount,
    string Status,
    string? Reference);

public record AddLineItemRequest(
    Guid ProductId,
    decimal Quantity,
    decimal? OverridePrice = null,
    Guid? OverrideApprovedBy = null);

public record AddPaymentRequest(
    string Method,
    decimal Amount,
    string? MobileNumber = null,
    string? Reference = null,
    string? GiftCardCode = null);

public record VoidLineItemRequest(Guid LineItemId);

public record VoidTransactionRequest(string Reason, Guid SupervisorId);

public record AttachCustomerRequest(Guid CustomerId);

public record SuspendTransactionRequest(string? Notes = null);

public record ReturnLineItemRequest(
    Guid OriginalLineItemId,
    decimal Quantity,
    string ReturnReason = "Other",
    string DamageDisposition = "RestoreToStock");

public record CreateReturnTransactionRequest(
    IEnumerable<ReturnLineItemRequest> Items,
    string RefundMethod,
    string? Notes = null);

public record ApplyDiscountRequest(decimal DiscountAmount);
