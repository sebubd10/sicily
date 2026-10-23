namespace BasicCommerce.Contracts.GiftCards;

public record GiftCardTransactionResponse(
    Guid Id,
    string TransactionType,
    decimal Amount,
    decimal BalanceAfter,
    Guid? SaleTransactionId,
    string? Notes,
    DateTime CreatedAt);

/// <param name="StoreId">Store the card is restricted to; null = usable at any store.</param>
public record GiftCardResponse(
    Guid Id,
    string Code,
    Guid? StoreId,
    decimal InitialBalance,
    decimal Balance,
    string CardStatus,
    DateTime? ExpiryDate,
    Guid? IssuedToCustomerId,
    string? CustomerName,
    Guid? IssuedInTransactionId,
    string? Notes,
    DateTime CreatedAt,
    IReadOnlyList<GiftCardTransactionResponse> Transactions);

public record GiftCardSummaryResponse(
    Guid Id,
    string Code,
    Guid? StoreId,
    Guid? IssuedToCustomerId,
    string? CustomerName,
    decimal Balance,
    string CardStatus,
    DateTime? ExpiryDate,
    DateTime CreatedAt);

public record GiftCardListResponse(
    IReadOnlyList<GiftCardSummaryResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

/// <param name="StoreId">Leave empty to make the card usable at every store.</param>
/// <param name="IssuedToCustomerId">Leave empty so anyone can use the card.</param>
public record IssueGiftCardRequest(
    Guid? StoreId,
    decimal Amount,
    DateTime? ExpiryDate,
    Guid? IssuedToCustomerId,
    string? Notes);

public record ReloadGiftCardRequest(decimal Amount, string? Notes);

/// <param name="StoreId">Store the card is restricted to; null = usable at any store.</param>
public record CheckBalanceResponse(
    string Code,
    decimal Balance,
    string CardStatus,
    DateTime? ExpiryDate,
    Guid? StoreId = null);
