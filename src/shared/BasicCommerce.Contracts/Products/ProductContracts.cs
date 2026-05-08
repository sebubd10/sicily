namespace BasicCommerce.Contracts.Products;

public record ProductTagResponse(Guid Id, string Name);

public record ProductResponse(
    Guid Id,
    string Sku,
    string Barcode,
    string? Plu,
    string Name,
    string NameBn,
    string? Description,
    Guid CategoryId,
    string CategoryName,
    decimal Price,
    string Currency,
    decimal? CostPrice,
    Guid VatRateId,
    string VatRateName,
    decimal VatRate,
    string UnitType,
    string? UnitLabel,
    bool IsWeightBased,
    bool IsPerishable,
    bool IsAgeRestricted,
    int? AgeRestrictionYears,
    bool IsEbtEligible,
    bool TrackInventory,
    int ReorderLevel,
    string Status,
    string? ImageUrl,
    Guid? ManufacturerId,
    string? ManufacturerName,
    IReadOnlyList<ProductTagResponse> Tags,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record ProductListResponse(
    IEnumerable<ProductResponse> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);

public record CreateProductRequest(
    string Sku,
    string Barcode,
    string? Plu,
    string Name,
    string NameBn,
    Guid CategoryId,
    decimal Price,
    Guid VatRateId,
    string UnitType = "Each",
    bool IsWeightBased = false,
    bool IsPerishable = false,
    bool IsAgeRestricted = false,
    int? AgeRestrictionYears = null,
    decimal? CostPrice = null,
    string? Description = null,
    string? UnitLabel = null,
    Guid? ManufacturerId = null,
    IEnumerable<Guid>? TagIds = null);

public record UpdateProductRequest(
    string Name,
    string NameBn,
    string? Description,
    Guid CategoryId,
    Guid VatRateId,
    string UnitType,
    string? UnitLabel,
    bool IsWeightBased,
    bool IsPerishable,
    bool IsAgeRestricted,
    int? AgeRestrictionYears,
    bool IsEbtEligible,
    bool TrackInventory,
    int ReorderLevel,
    string? ImageUrl,
    decimal? CostPrice,
    Guid? ManufacturerId = null,
    IEnumerable<Guid>? TagIds = null);

public record SetProductTagsRequest(IEnumerable<Guid> TagIds);

public record UpdateProductPriceRequest(decimal NewPrice);

public record VatRateResponse(
    Guid Id,
    string Name,
    string Code,
    decimal Rate,
    bool IsDefault,
    string Status);

public record CategoryResponse(
    Guid Id,
    string Name,
    string NameBn,
    string? Description,
    Guid? ParentCategoryId,
    string? ParentCategoryName,
    int SortOrder,
    string Status,
    int ChildCount);

public record UpdateCategoryRequest(
    string Name,
    string NameBn,
    string? Description,
    int SortOrder);
