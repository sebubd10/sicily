namespace BasicCommerce.Contracts.Manufacturers;

public record CountryResponse(string Code, string Name);

public record ManufacturerResponse(
    Guid Id,
    string Name,
    string? Code,
    string? Country,
    string? Website,
    string? ContactEmail,
    string? Notes,
    string Status);

public record CreateManufacturerRequest(
    string Name,
    string? Code = null,
    string? Country = null,
    string? Website = null,
    string? ContactEmail = null,
    string? Notes = null);

public record UpdateManufacturerRequest(
    string Name,
    string? Code = null,
    string? Country = null,
    string? Website = null,
    string? ContactEmail = null,
    string? Notes = null);
