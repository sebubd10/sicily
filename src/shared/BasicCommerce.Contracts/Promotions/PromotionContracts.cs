namespace BasicCommerce.Contracts.Promotions;

public record PromotionResponse(
    Guid Id,
    string Name,
    string? Description,
    string Type,
    string PromotionStatus,
    Guid? ProductId,
    Guid? CategoryId,
    Guid? StoreId,
    decimal? DiscountPercentage,
    decimal? DiscountAmount,
    int? BuyQuantity,
    int? GetQuantity,
    decimal? MinimumCartValue,
    string? CouponCode,
    bool RequiresCoupon,
    DateTime? StartsAt,
    DateTime? EndsAt,
    int? MaxUses,
    int UsedCount,
    DateTime CreatedAt);

public record PromotionSummaryResponse(
    Guid Id,
    string Name,
    string Type,
    string PromotionStatus,
    decimal? DiscountPercentage,
    decimal? DiscountAmount,
    DateTime? StartsAt,
    DateTime? EndsAt);

public record PromotionListResponse(
    IReadOnlyList<PromotionSummaryResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record CreatePromotionRequest(
    string Name,
    string? Description,
    string Type,
    Guid? ProductId,
    Guid? CategoryId,
    Guid? StoreId,
    decimal? DiscountPercentage,
    decimal? DiscountAmount,
    int? BuyQuantity,
    int? GetQuantity,
    decimal? MinimumCartValue,
    string? CouponCode,
    bool RequiresCoupon,
    DateTime? StartsAt,
    DateTime? EndsAt,
    int? MaxUses);

public record ApplyCouponRequest(string CouponCode);

public record AppliedPromotionResponse(
    Guid PromotionId,
    string PromotionName,
    decimal DiscountAmount);
