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

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Role,
    Guid? StoreId = null,
    string? PhoneNumber = null);

public record UserDetailResponse(
    Guid Id,
    string EmployeeCode,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    Guid? StoreId,
    string? StoreName,
    string? PhoneNumber,
    string Status,
    DateTime? LastLoginAt,
    Guid? UserTypeId,
    string? UserTypeName,
    bool IsLocked);
