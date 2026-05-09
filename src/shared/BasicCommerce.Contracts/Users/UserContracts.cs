namespace BasicCommerce.Contracts.Users;

public record UserListResponse(
    Guid Id,
    string EmployeeCode,
    string FullName,
    string Email,
    string Role,
    string? StoreName,
    string Status,
    DateTime? LastLoginAt,
    Guid? UserTypeId = null,
    string? UserTypeName = null,
    bool IsLocked = false);

public record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role,
    Guid? StoreId = null,
    string? PhoneNumber = null);
