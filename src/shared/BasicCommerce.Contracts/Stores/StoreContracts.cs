namespace BasicCommerce.Contracts.Stores;

public record StoreResponse(
    Guid Id,
    string Name,
    string Code,
    string Address,
    string? Phone,
    string? Email,
    string Status,
    int TerminalCount);

public record CreateStoreRequest(
    string Name,
    string Code,
    string AddressLine1,
    string City,
    string District,
    string PostalCode,
    string? Phone = null,
    string? Email = null);

public record TerminalResponse(
    Guid Id,
    string Name,
    string Code,
    string Type,
    string Status,
    string? CurrentCashierName);
