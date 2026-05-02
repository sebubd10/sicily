namespace BasicCommerce.Contracts.Users;

public record UserListResponse(
    Guid Id,
    string EmployeeCode,
    string FullName,
    string Email,
    string Role,
    string? StoreName,
    bool IsActive,
    DateTime? LastLoginAt);

public record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role,
    Guid? StoreId = null,
    string? PhoneNumber = null);
