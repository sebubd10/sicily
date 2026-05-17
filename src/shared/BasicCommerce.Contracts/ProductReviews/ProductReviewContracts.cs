namespace BasicCommerce.Contracts.ProductReviews;

public record ProductReviewDetailResponse(
    Guid Id,
    string Comment,
    bool IsAdminReply,
    string? CommenterName,
    DateTime CreatedAt);

public record ProductReviewResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid? CustomerId,
    string CustomerName,
    string Title,
    string ReviewText,
    int Rating,
    bool IsApproved,
    bool IsVerifiedPurchase,
    int HelpfulYesTotal,
    int HelpfulNoTotal,
    string Status,
    IReadOnlyList<ProductReviewDetailResponse> Details,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record ProductReviewSummaryResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid? CustomerId,
    string CustomerName,
    string Title,
    int Rating,
    bool IsApproved,
    bool IsVerifiedPurchase,
    int HelpfulYesTotal,
    int HelpfulNoTotal,
    int DetailCount,
    string Status,
    DateTime CreatedAt);

public record ProductReviewListResponse(
    IEnumerable<ProductReviewSummaryResponse> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);

public record ProductRatingSummaryResponse(
    Guid ProductId,
    decimal AverageRating,
    int ReviewCount);

public record SubmitProductReviewRequest(
    Guid ProductId,
    Guid? StoreId,
    string CustomerName,
    string Title,
    string ReviewText,
    int Rating);

public record AddReviewDetailRequest(string Comment);

public record MarkReviewHelpfulRequest(bool IsHelpful);
