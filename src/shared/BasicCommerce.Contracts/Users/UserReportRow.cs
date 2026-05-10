namespace BasicCommerce.Contracts.Users;

public record UserReportRow(
    string EmployeeCode,
    string FullName,
    string Email,
    string Role,
    string? UserTypeName,
    string? StoreName,
    string Status,
    bool IsLocked,
    DateTime? LastLoginAt);
