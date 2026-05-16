namespace BasicCommerce.Contracts.Reports;

public record ProductReportRow(
    string Sku,
    string Barcode,
    string Name,
    string Category,
    decimal Price,
    decimal? CostPrice,
    string Currency,
    decimal VatRate,
    string UnitType,
    string? Manufacturer,
    string Status);
