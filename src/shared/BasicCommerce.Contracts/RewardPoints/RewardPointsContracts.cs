namespace BasicCommerce.Contracts.RewardPoints;

public record RewardPointsSettingsResponse(
    Guid Id,
    bool IsEnabled,
    decimal ExchangeRate,
    int MinimumPointsToUse,
    int MaximumPointsPerOrder,
    decimal MaximumRedeemedRate,
    decimal PurchaseSpendPerPoint,
    int PointsEarnedPerSpend,
    int PurchasePointsValidityDays,
    decimal MinimumOrderTotalForPoints,
    int PointsForRegistration,
    int RegistrationPointsValidityDays,
    bool ActivatePointsImmediately,
    bool DisplayHowMuchWillBeEarned,
    bool PointsAccumulatedForAllStores);

public record UpdateRewardPointsSettingsRequest(
    bool IsEnabled,
    decimal ExchangeRate,
    int MinimumPointsToUse,
    int MaximumPointsPerOrder,
    decimal MaximumRedeemedRate,
    decimal PurchaseSpendPerPoint,
    int PointsEarnedPerSpend,
    int PurchasePointsValidityDays,
    decimal MinimumOrderTotalForPoints,
    int PointsForRegistration,
    int RegistrationPointsValidityDays,
    bool ActivatePointsImmediately,
    bool DisplayHowMuchWillBeEarned,
    bool PointsAccumulatedForAllStores);

public record RewardPointsEntryResponse(
    Guid Id,
    int Points,
    string EntryType,
    bool IsActivated,
    DateTime? ExpiresAt,
    Guid? TransactionId,
    string? Notes,
    DateTime CreatedAt);

public record RewardPointsAccountResponse(
    Guid Id,
    Guid CustomerId,
    Guid? StoreId,
    int TotalEarnedPoints,
    int UsedPoints,
    int ExpiredPoints,
    int PendingPoints,
    int AvailablePoints,
    IReadOnlyList<RewardPointsEntryResponse> RecentEntries);

public record RedeemPointsRequest(int Points);

public record CalculateEarnedPointsRequest(decimal OrderTotal);

public record CalculateEarnedPointsResponse(int Points, decimal PointsValue, bool WillEarnPoints);

public record ManualAdjustRewardPointsRequest(Guid CustomerId, Guid? StoreId, int Points, string Notes);

/// <summary>Reward points snapshot for a customer at checkout.</summary>
/// <param name="AvailablePoints">Spendable points right now (expired points excluded).</param>
/// <param name="AvailableValue">Currency value of AvailablePoints.</param>
/// <param name="PendingPoints">Earned but not yet activated, so not spendable yet.</param>
/// <param name="MaxRedeemablePoints">Most points usable on this order after all programme limits.</param>
/// <param name="MaxRedeemableAmount">Currency value of MaxRedeemablePoints, never more than the order total.</param>
/// <param name="PointsToEarn">Points this order will earn on the part not paid with points.</param>
public record CheckoutRewardPointsResponse(
    bool ProgramEnabled,
    int AvailablePoints,
    decimal AvailableValue,
    int PendingPoints,
    decimal ExchangeRate,
    int MinimumPointsToUse,
    int MaxRedeemablePoints,
    decimal MaxRedeemableAmount,
    int PointsToEarn,
    bool PointsActivateImmediately);
