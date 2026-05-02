namespace BasicCommerce.Contracts.Transactions;

public record TransactionResponse(
    Guid Id,
    string TransactionNumber,
    string Status,
    string Type,
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
    DateTime CreatedAt,
    DateTime? CompletedAt);

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
    bool IsPriceOverridden);

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
    string? Reference = null);

public record VoidLineItemRequest(Guid LineItemId);

public record VoidTransactionRequest(string Reason, Guid SupervisorId);

public record AttachCustomerRequest(Guid CustomerId);

public record SuspendTransactionRequest(string? Notes = null);

public record ReturnLineItemRequest(Guid OriginalLineItemId, decimal Quantity);

public record CreateReturnTransactionRequest(
    IEnumerable<ReturnLineItemRequest> Items,
    string RefundMethod,
    string? Notes = null);

public record ApplyDiscountRequest(decimal DiscountAmount);
