namespace BasicCommerce.Contracts.Suppliers;

public record SupplierProductResponse(
    Guid Id,
    Guid SupplierId,
    string SupplierName,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    string? SupplierSku,
    decimal UnitCost,
    string CurrencyCode,
    int? MinOrderQuantity,
    int? LeadTimeDays,
    string? Notes,
    DateTime PriceLastConfirmedAt);

public record SupplierProductListResponse(
    IReadOnlyList<SupplierProductResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record UpsertSupplierProductRequest(
    Guid ProductId,
    decimal UnitCost,
    string? SupplierSku = null,
    int? MinOrderQuantity = null,
    int? LeadTimeDays = null,
    string? Notes = null,
    string CurrencyCode = "BDT");
