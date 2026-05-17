namespace BasicCommerce.Domain.ValueObjects;

public sealed record RefreshToken(
    string Token,
    DateTime ExpiresAt,
    bool IsRevoked = false)
{
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}
