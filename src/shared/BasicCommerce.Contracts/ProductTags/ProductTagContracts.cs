namespace BasicCommerce.Contracts.ProductTags;

public record ProductTagDetailResponse(
    Guid Id,
    string Name,
    int TaggedProductsCount,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record ProductTagListResponse(
    IEnumerable<ProductTagDetailResponse> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);

public record CreateProductTagRequest(string Name);

public record UpdateProductTagRequest(string Name);

public record BulkDeleteProductTagsRequest(IEnumerable<Guid> Ids);
