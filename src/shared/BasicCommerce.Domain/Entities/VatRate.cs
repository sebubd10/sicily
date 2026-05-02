namespace BasicCommerce.Domain.Entities;

/// <summary>
/// NBR (National Board of Revenue) VAT rate configuration for Bangladesh.
/// </summary>
public class VatRate : TenantEntity
{
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public decimal Rate { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; } = true;

    private VatRate() { }

    public static VatRate Create(Guid tenantId, string name, string code, decimal rate,
        bool isDefault = false)
    {
        if (rate < 0 || rate > 100)
            throw new ArgumentOutOfRangeException(nameof(rate), "VAT rate must be between 0 and 100.");

        return new VatRate
        {
            TenantId = tenantId,
            Name = name,
            Code = code.ToUpperInvariant(),
            Rate = rate,
            IsDefault = isDefault
        };
    }

    public void UpdateRate(decimal newRate)
    {
        if (newRate < 0 || newRate > 100)
            throw new ArgumentOutOfRangeException(nameof(newRate));
        Rate = newRate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;
}
