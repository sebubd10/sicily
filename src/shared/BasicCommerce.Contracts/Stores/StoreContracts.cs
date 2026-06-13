namespace BasicCommerce.Contracts.Stores;

public record StoreResponse(
    Guid Id,
    string Name,
    string Code,
    string Address,
    string? Phone,
    string? Email,
    string Status,
    int TerminalCount,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string District,
    string PostalCode,
    string Country,
    string OpeningTime,
    string ClosingTime);

public record CreateStoreRequest(
    string Name,
    string Code,
    string AddressLine1,
    string City,
    string District,
    string PostalCode,
    string? Phone = null,
    string? Email = null);

public record UpdateStoreRequest(
    string Name,
    string AddressLine1,
    string City,
    string District,
    string PostalCode,
    string? AddressLine2 = null,
    string? Phone = null,
    string? Email = null,
    string OpeningTime = "08:00",
    string ClosingTime = "22:00");

public record TerminalResponse(
    Guid Id,
    Guid StoreId,
    string StoreName,
    string Name,
    string Code,
    string Type,
    string Status,
    string OperationalStatus,
    string? CurrentCashierName);

public record CreateTerminalRequest(
    Guid StoreId,
    string Name,
    string Code,
    string Type);

public record UpdateTerminalRequest(
    string Name,
    string Code,
    string Type);
