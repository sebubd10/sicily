using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Entities;

public class User : TenantEntity
{
    public string EmployeeCode { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public Guid? StoreId { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? GoogleId { get; private set; }
    public string? MicrosoftId { get; private set; }
    public AuthProvider AuthProvider { get; private set; } = AuthProvider.Local;
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public string PreferredLanguage { get; private set; } = "en";

    public string FullName => $"{FirstName} {LastName}";
    public bool IsLocked => LockedUntil.HasValue && LockedUntil > DateTime.UtcNow;

    private User() { }

    public static User Create(Guid tenantId, string employeeCode, string firstName,
        string lastName, string email, string passwordHash, UserRole role,
        Guid? storeId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User
        {
            TenantId = tenantId,
            EmployeeCode = employeeCode,
            FirstName = firstName,
            LastName = lastName,
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role,
            StoreId = storeId
        };
    }

    public static User CreateWithExternalProvider(Guid tenantId, string firstName,
        string lastName, string email, AuthProvider provider, string externalId,
        UserRole role = UserRole.Customer)
    {
        var user = new User
        {
            TenantId = tenantId,
            EmployeeCode = Guid.NewGuid().ToString("N")[..8].ToUpper(),
            FirstName = firstName,
            LastName = lastName,
            Email = email.ToLowerInvariant(),
            PasswordHash = string.Empty,
            Role = role,
            AuthProvider = provider
        };

        if (provider == AuthProvider.Google) user.GoogleId = externalId;
        if (provider == AuthProvider.Microsoft) user.MicrosoftId = externalId;

        return user;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
            LockedUntil = DateTime.UtcNow.AddMinutes(30);
    }

    public void Unlock()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
