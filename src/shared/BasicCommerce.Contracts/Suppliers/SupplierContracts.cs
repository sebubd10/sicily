namespace BasicCommerce.Contracts.Suppliers;

public record SupplierResponse(
    Guid Id,
    string Name,
    string Code,
    string? ContactName,
    string? Email,
    string? Phone,
    string? AddressLine1,
    string? City,
    int LeadTimeDays,
    string? Notes,
    Guid? ManufacturerId,
    string? ManufacturerName,
    string Status);

public record CreateSupplierRequest(
    string Name,
    string Code,
    string? ContactName = null,
    string? Email = null,
    string? Phone = null,
    string? AddressLine1 = null,
    string? AddressLine2 = null,
    string? City = null,
    string? District = null,
    string? PostalCode = null,
    string? Country = null,
    int LeadTimeDays = 0,
    string? Notes = null,
    Guid? ManufacturerId = null);

public record UpdateSupplierRequest(
    string Name,
    string? ContactName = null,
    string? Email = null,
    string? Phone = null,
    string? AddressLine1 = null,
    string? AddressLine2 = null,
    string? City = null,
    string? District = null,
    string? PostalCode = null,
    string? Country = null,
    int LeadTimeDays = 0,
    string? Notes = null,
    Guid? ManufacturerId = null);
