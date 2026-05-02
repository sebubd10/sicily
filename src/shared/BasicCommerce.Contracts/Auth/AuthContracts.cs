namespace BasicCommerce.Contracts.Auth;

public record LoginRequest(string Email, string Password, string? TenantSlug = null);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string TokenType,
    UserDto User);

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    Guid TenantId,
    Guid? StoreId,
    string AuthProvider,
    string PreferredLanguage);

public record RegisterCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Phone = null);

public record ExternalAuthCallbackRequest(
    string Provider,
    string Code,
    string? State = null);
