namespace BasicCommerce.Contracts.Manufacturers;

public record ManufacturerReportRow(
    string Name,
    string? Code,
    string? Country,
    string? Website,
    string? ContactEmail,
    string? Notes,
    string Status);
