namespace BasicCommerce.Contracts.Products;

public record ProductResponse(
    Guid Id,
    string Sku,
    string Barcode,
    string? Plu,
    string Name,
    string NameBn,
    string CategoryId,
    string CategoryName,
    decimal Price,
    string Currency,
    decimal VatRate,
    string UnitType,
    bool IsWeightBased,
    bool IsAgeRestricted,
    int? AgeRestrictionYears,
    bool IsActive,
    string? ImageUrl);

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
    bool IsAgeRestricted = false,
    int? AgeRestrictionYears = null,
    decimal? CostPrice = null);

public record UpdateProductPriceRequest(decimal NewPrice);
