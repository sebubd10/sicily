using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.ValueObjects;

namespace BasicCommerce.Domain.Entities;

public class Customer : TenantEntity
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public Address? Address { get; private set; }
    public decimal CreditLimit { get; private set; }
    public decimal CurrentBalance { get; private set; }
    public int LoyaltyPoints { get; private set; }

    public decimal AvailableCredit => CreditLimit - CurrentBalance;
    public bool HasCredit => AvailableCredit > 0;

    private Customer() { }

    public static Customer Create(Guid tenantId, string name, string? email = null,
        string? phone = null, decimal creditLimit = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Customer
        {
            TenantId = tenantId,
            Code = GenerateCode(),
            Name = name,
            Email = email?.ToLowerInvariant(),
            Phone = phone,
            CreditLimit = creditLimit
        };
    }

    public void AddLoyaltyPoints(int points)
    {
        if (points <= 0) return;
        LoyaltyPoints += points;
    }

    public void RedeemLoyaltyPoints(int points)
    {
        if (points > LoyaltyPoints)
            throw new InvalidOperationException("Insufficient loyalty points.");
        LoyaltyPoints -= points;
    }

    public void AddToBalance(decimal amount) => CurrentBalance += amount;

    public void ReduceBalance(decimal amount)
    {
        CurrentBalance = Math.Max(0, CurrentBalance - amount);
    }

    public void Update(string name, string? email, string? phone, Address? address)
    {
        Name = name;
        Email = email?.ToLowerInvariant();
        Phone = phone;
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCreditLimit(decimal newLimit) => CreditLimit = newLimit;

    public void Deactivate()
    {
        Status = EntityStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = EntityStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateCode() =>
        $"CUST-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
}
