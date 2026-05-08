namespace BasicCommerce.Contracts.GiftCards;

public record GiftCardTransactionResponse(
    Guid Id,
    string TransactionType,
    decimal Amount,
    decimal BalanceAfter,
    Guid? SaleTransactionId,
    string? Notes,
    DateTime CreatedAt);

public record GiftCardResponse(
    Guid Id,
    string Code,
    Guid StoreId,
    decimal InitialBalance,
    decimal Balance,
    string CardStatus,
    DateTime? ExpiryDate,
    Guid? IssuedToCustomerId,
    Guid? IssuedInTransactionId,
    string? Notes,
    DateTime CreatedAt,
    IReadOnlyList<GiftCardTransactionResponse> Transactions);

public record GiftCardSummaryResponse(
    Guid Id,
    string Code,
    Guid StoreId,
    decimal Balance,
    string CardStatus,
    DateTime? ExpiryDate,
    DateTime CreatedAt);

public record GiftCardListResponse(
    IReadOnlyList<GiftCardSummaryResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record IssueGiftCardRequest(
    Guid StoreId,
    decimal Amount,
    DateTime? ExpiryDate,
    Guid? IssuedToCustomerId,
    string? Notes);

public record ReloadGiftCardRequest(decimal Amount, string? Notes);

public record CheckBalanceResponse(
    string Code,
    decimal Balance,
    string CardStatus,
    DateTime? ExpiryDate);
