using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Entities;

public class ProductImage : TenantEntity
{
    public Guid ProductId { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }

    /// <summary>
    /// Publicly accessible URL. For uploaded files this is the served path
    /// (e.g. /uploads/products/{tenantId}/{storedFileName}); for external
    /// images it is the URL the user provided.
    /// </summary>
    public string Url { get; private set; } = default!;

    /// <summary>Physical file name on disk. Null for external-URL images.</summary>
    public string? StoredFileName { get; private set; }

    public bool IsUploaded { get; private set; }
    public int SortOrder { get; private set; }

    public Product Product { get; private set; } = default!;

    private ProductImage() { }

    public static ProductImage CreateFromUrl(
        Guid tenantId, Guid productId,
        string title, string url,
        string? description = null, int sortOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        return new ProductImage
        {
            TenantId     = tenantId,
            ProductId    = productId,
            Title        = title.Trim(),
            Description  = description?.Trim(),
            Url          = url.Trim(),
            IsUploaded   = false,
            SortOrder    = sortOrder,
        };
    }

    public static ProductImage CreateFromUpload(
        Guid tenantId, Guid productId,
        string title, string url, string storedFileName,
        string? description = null, int sortOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentException.ThrowIfNullOrWhiteSpace(storedFileName);
        return new ProductImage
        {
            TenantId      = tenantId,
            ProductId     = productId,
            Title         = title.Trim(),
            Description   = description?.Trim(),
            Url           = url,
            StoredFileName = storedFileName,
            IsUploaded    = true,
            SortOrder     = sortOrder,
        };
    }

    public void Update(string title, string? description)
    {
        Title       = title.Trim();
        Description = description?.Trim();
        UpdatedAt   = DateTime.UtcNow;
    }

    public void SetSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDeleted()
    {
        Status    = EntityStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;
    }
}
