namespace BasicCommerce.Contracts.Products;

public record ProductImageResponse(
    Guid Id,
    Guid ProductId,
    string Title,
    string? Description,
    string Url,
    bool IsUploaded,
    int SortOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record AddProductImageByUrlRequest(
    string Title,
    string Url,
    string? Description = null,
    int SortOrder = 0);

public record AddProductImageByUploadRequest(
    string Title,
    string? Description = null,
    int SortOrder = 0);

public record UpdateProductImageRequest(
    string Title,
    string? Description);

public record ReorderProductImagesRequest(
    IEnumerable<ProductImageOrderItem> Items);

public record ProductImageOrderItem(Guid Id, int SortOrder);
