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
