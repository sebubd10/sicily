namespace BasicCommerce.Contracts.Till;

public record PettyTransactionResponse(
    Guid Id,
    string Type,
    decimal Amount,
    string Reason,
    Guid PerformedBy,
    DateTime CreatedAt);

public record TillSessionResponse(
    Guid Id,
    Guid StoreId,
    Guid TerminalId,
    Guid OpenedBy,
    Guid? ClosedBy,
    decimal OpeningFloat,
    decimal? ClosingBalance,
    decimal? ClosingVariance,
    decimal? ExpectedClosingBalance,
    string SessionStatus,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    string? Notes,
    IReadOnlyList<PettyTransactionResponse> PettyTransactions);

public record TillSessionSummaryResponse(
    Guid Id,
    Guid StoreId,
    string? StoreName,
    Guid TerminalId,
    string? TerminalName,
    Guid OpenedBy,
    string? OpenedByName,
    Guid? ClosedBy,
    string? ClosedByName,
    decimal OpeningFloat,
    string SessionStatus,
    DateTime OpenedAt,
    DateTime? ClosedAt);

public record TillSessionListResponse(
    IReadOnlyList<TillSessionSummaryResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record OpenTillSessionRequest(
    Guid StoreId,
    Guid TerminalId,
    decimal OpeningFloat,
    string? Notes);

public record CloseTillSessionRequest(
    decimal ClosingBalance,
    string? Notes);

public record PettyCashRequest(
    decimal Amount,
    string Reason);

public record TillReportResponse(
    Guid SessionId,
    decimal OpeningFloat,
    decimal CashSalesTotal,
    decimal CardSalesTotal,
    decimal GiftCardSalesTotal,
    decimal TotalRefunds,
    decimal PettyCashIn,
    decimal PettyCashOut,
    decimal ExpectedCash,
    decimal? ActualCash,
    decimal? Variance,
    bool IsFinal);
