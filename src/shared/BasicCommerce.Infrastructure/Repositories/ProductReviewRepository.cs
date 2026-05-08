using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class ProductReviewRepository
    : TenantRepository<ProductReview>, IProductReviewRepository
{
    public ProductReviewRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<(IEnumerable<ProductReview> Items, int TotalCount)> GetPagedAsync(
        Guid tenantId, int page, int pageSize,
        Guid? productId = null, Guid? storeId = null,
        bool? isApproved = null, string? search = null,
        CancellationToken ct = default)
    {
        var query = Db.ProductReviews
            .Include(r => r.Product)
            .Include(r => r.Customer)
            .Where(r => r.TenantId == tenantId);

        if (productId.HasValue)
            query = query.Where(r => r.ProductId == productId.Value);

        if (storeId.HasValue)
            query = query.Where(r => r.StoreId == storeId.Value);

        if (isApproved.HasValue)
            query = query.Where(r => r.IsApproved == isApproved.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r =>
                r.Title.Contains(search) ||
                r.ReviewText.Contains(search) ||
                r.CustomerName.Contains(search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<ProductReview?> GetWithDetailsAsync(Guid tenantId, Guid reviewId,
        CancellationToken ct = default) =>
        await Db.ProductReviews
            .Include(r => r.Product)
            .Include(r => r.Customer)
            .Include(r => r.Details.Where(d => d.Status != EntityStatus.Deleted)
                .OrderBy(d => d.CreatedAt))
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == reviewId, ct);

    public async Task<(IEnumerable<ProductReview> Items, int TotalCount)> GetByProductAsync(
        Guid tenantId, Guid productId, int page, int pageSize,
        bool approvedOnly = true, CancellationToken ct = default)
    {
        var query = Db.ProductReviews
            .Include(r => r.Details.Where(d => d.Status != EntityStatus.Deleted)
                .OrderBy(d => d.CreatedAt))
            .Where(r => r.TenantId == tenantId && r.ProductId == productId);

        if (approvedOnly)
            query = query.Where(r => r.IsApproved);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(decimal AverageRating, int ReviewCount)> GetProductRatingSummaryAsync(
        Guid tenantId, Guid productId, CancellationToken ct = default)
    {
        var stats = await Db.ProductReviews
            .Where(r => r.TenantId == tenantId && r.ProductId == productId && r.IsApproved)
            .GroupBy(_ => 1)
            .Select(g => new { Avg = (decimal?)g.Average(r => r.Rating), Count = g.Count() })
            .FirstOrDefaultAsync(ct);

        return (stats?.Avg ?? 0m, stats?.Count ?? 0);
    }

    public async Task<ProductReviewHelpfulness?> GetHelpfulnessVoteAsync(
        Guid reviewId, Guid customerId, CancellationToken ct = default) =>
        await Db.ProductReviewHelpfulnesses.FirstOrDefaultAsync(
            h => h.ProductReviewId == reviewId && h.CustomerId == customerId, ct);

    public async Task AddHelpfulnessVoteAsync(ProductReviewHelpfulness vote,
        CancellationToken ct = default) =>
        await Db.ProductReviewHelpfulnesses.AddAsync(vote, ct);

    public void UpdateHelpfulnessVote(ProductReviewHelpfulness vote) =>
        Db.ProductReviewHelpfulnesses.Update(vote);
}
