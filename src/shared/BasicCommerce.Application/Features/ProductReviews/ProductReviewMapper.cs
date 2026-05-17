using BasicCommerce.Contracts.ProductReviews;
using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Application.Features.ProductReviews;

internal static class ProductReviewMapper
{
    internal static ProductReviewResponse ToResponse(ProductReview r) => new(
        r.Id,
        r.ProductId,
        r.Product?.Name ?? string.Empty,
        r.CustomerId,
        r.CustomerName,
        r.Title,
        r.ReviewText,
        r.Rating,
        r.IsApproved,
        r.IsVerifiedPurchase,
        r.HelpfulYesTotal,
        r.HelpfulNoTotal,
        r.Status.ToString(),
        r.Details.Select(d => new ProductReviewDetailResponse(
            d.Id, d.Comment, d.IsAdminReply, d.CommenterName, d.CreatedAt)).ToList(),
        r.CreatedAt,
        r.UpdatedAt);

    internal static ProductReviewSummaryResponse ToSummary(ProductReview r) => new(
        r.Id,
        r.ProductId,
        r.Product?.Name ?? string.Empty,
        r.CustomerId,
        r.CustomerName,
        r.Title,
        r.Rating,
        r.IsApproved,
        r.IsVerifiedPurchase,
        r.HelpfulYesTotal,
        r.HelpfulNoTotal,
        r.Details.Count,
        r.Status.ToString(),
        r.CreatedAt);
}
