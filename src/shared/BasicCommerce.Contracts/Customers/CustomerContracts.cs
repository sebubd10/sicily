namespace BasicCommerce.Contracts.Customers;

public record CustomerResponse(
    Guid Id,
    string Code,
    string Name,
    string? Email,
    string? Phone,
    string? AddressLine1,
    string? City,
    int LoyaltyPoints,
    decimal CreditLimit,
    decimal CurrentBalance,
    decimal AvailableCredit,
    bool IsActive,
    DateTime CreatedAt);

public record RegisterCustomerRequest(
    string Name,
    string? Email = null,
    string? Phone = null,
    decimal CreditLimit = 0,
    string? AddressLine1 = null,
    string? City = null,
    string? District = null,
    string? PostalCode = null);

public record UpdateCustomerRequest(
    string Name,
    string? Email = null,
    string? Phone = null,
    string? AddressLine1 = null,
    string? City = null,
    string? District = null,
    string? PostalCode = null);

public record CreditAccountResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CustomerCode,
    string StoreName,
    decimal CreditLimit,
    decimal OutstandingBalance,
    decimal AvailableCredit,
    bool IsActive,
    DateTime? LastPaymentAt,
    IEnumerable<CreditTransactionRow> RecentHistory);

public record CreditTransactionRow(
    Guid Id,
    string Type,
    decimal Amount,
    string Description,
    string? Reference,
    DateTime CreatedAt);

public record RecordCreditPaymentRequest(decimal Amount, string? Reference = null);

public record UpdateCreditLimitRequest(decimal CreditLimit);

public record AdjustLoyaltyPointsRequest(int Points, string Reason);
