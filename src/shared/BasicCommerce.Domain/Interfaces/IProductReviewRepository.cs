using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface IProductReviewRepository : ITenantRepository<ProductReview>
{
    Task<(IEnumerable<ProductReview> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId, int page, int pageSize,
        Guid? productId = null, Guid? storeId = null,
        bool? isApproved = null, string? search = null,
        CancellationToken ct = default);

    Task<ProductReview?> GetWithDetailsAsync(Guid tenantId, Guid reviewId,
        CancellationToken ct = default);

    Task<(IEnumerable<ProductReview> Items, int TotalCount)> GetByProductAsync(
        Guid tenantId, Guid productId, int page, int pageSize,
        bool approvedOnly = true, CancellationToken ct = default);

    Task<(decimal AverageRating, int ReviewCount)> GetProductRatingSummaryAsync(
        Guid tenantId, Guid productId, CancellationToken ct = default);

    Task<ProductReviewHelpfulness?> GetHelpfulnessVoteAsync(
        Guid reviewId, Guid customerId, CancellationToken ct = default);

    Task AddHelpfulnessVoteAsync(ProductReviewHelpfulness vote, CancellationToken ct = default);
    void UpdateHelpfulnessVote(ProductReviewHelpfulness vote);
}
