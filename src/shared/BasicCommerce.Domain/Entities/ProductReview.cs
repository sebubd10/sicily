using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Entities;

public class ProductReview : TenantEntity
{
    public Guid ProductId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Guid? StoreId { get; private set; }
    public string CustomerName { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string ReviewText { get; private set; } = default!;
    public int Rating { get; private set; }
    public bool IsApproved { get; private set; }
    public bool IsVerifiedPurchase { get; private set; }
    public int HelpfulYesTotal { get; private set; }
    public int HelpfulNoTotal { get; private set; }

    public Product? Product { get; private set; }
    public Customer? Customer { get; private set; }

    private readonly List<ProductReviewDetail> _details = [];
    public IReadOnlyCollection<ProductReviewDetail> Details => _details.AsReadOnly();

    private ProductReview() { }

    public static ProductReview Create(
        Guid tenantId, Guid productId, Guid? customerId, Guid? storeId,
        string customerName, string title, string reviewText, int rating,
        bool requireApproval = true, bool isVerifiedPurchase = false) =>
        new()
        {
            TenantId = tenantId,
            ProductId = productId,
            CustomerId = customerId,
            StoreId = storeId,
            CustomerName = customerName.Trim(),
            Title = title.Trim(),
            ReviewText = reviewText.Trim(),
            Rating = Math.Clamp(rating, 1, 5),
            IsApproved = !requireApproval,
            IsVerifiedPurchase = isVerifiedPurchase
        };

    public void Approve()
    {
        IsApproved = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        IsApproved = false;
        Status = EntityStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public ProductReviewDetail AddDetail(
        string comment, bool isAdminReply, Guid? userId = null, string? commenterName = null)
    {
        var detail = ProductReviewDetail.Create(Id, comment, isAdminReply, userId, commenterName);
        _details.Add(detail);
        UpdatedAt = DateTime.UtcNow;
        return detail;
    }

    public void IncrementHelpfulVote(bool isHelpful)
    {
        if (isHelpful) HelpfulYesTotal++;
        else HelpfulNoTotal++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeHelpfulVote(bool oldVote, bool newVote)
    {
        if (oldVote) HelpfulYesTotal = Math.Max(0, HelpfulYesTotal - 1);
        else HelpfulNoTotal = Math.Max(0, HelpfulNoTotal - 1);

        if (newVote) HelpfulYesTotal++;
        else HelpfulNoTotal++;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        Status = EntityStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class ProductReviewDetail : BaseEntity
{
    public Guid ProductReviewId { get; private set; }
    public string Comment { get; private set; } = default!;
    public bool IsAdminReply { get; private set; }
    public Guid? UserId { get; private set; }
    public string? CommenterName { get; private set; }

    private ProductReviewDetail() { }

    internal static ProductReviewDetail Create(Guid reviewId, string comment,
        bool isAdminReply, Guid? userId, string? commenterName) =>
        new()
        {
            ProductReviewId = reviewId,
            Comment = comment.Trim(),
            IsAdminReply = isAdminReply,
            UserId = userId,
            CommenterName = commenterName?.Trim()
        };

    public void Delete()
    {
        Status = EntityStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class ProductReviewHelpfulness : BaseEntity
{
    public Guid ProductReviewId { get; private set; }
    public Guid CustomerId { get; private set; }
    public bool IsHelpful { get; private set; }

    private ProductReviewHelpfulness() { }

    public static ProductReviewHelpfulness Create(Guid reviewId, Guid customerId, bool isHelpful) =>
        new() { ProductReviewId = reviewId, CustomerId = customerId, IsHelpful = isHelpful };

    public void ChangeVote(bool isHelpful)
    {
        IsHelpful = isHelpful;
        UpdatedAt = DateTime.UtcNow;
    }
}
