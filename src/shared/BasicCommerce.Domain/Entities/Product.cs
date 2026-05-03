using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.ValueObjects;

namespace BasicCommerce.Domain.Entities;

public class Product : TenantEntity
{
    public string Sku { get; private set; } = default!;
    public string Barcode { get; private set; } = default!;
    public string? Plu { get; private set; }
    public string Name { get; private set; } = default!;
    public string NameBn { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public Money Price { get; private set; } = default!;
    public Money? CostPrice { get; private set; }
    public UnitType UnitType { get; private set; }
    public string? UnitLabel { get; private set; }
    public Guid VatRateId { get; private set; }
    public bool IsWeightBased { get; private set; }
    public bool IsAgeRestricted { get; private set; }
    public int? AgeRestrictionYears { get; private set; }
    public bool IsEbtEligible { get; private set; }
    public bool TrackInventory { get; private set; } = true;
    public int ReorderLevel { get; private set; }
    public string? ImageUrl { get; private set; }


    public Category? Category { get; private set; }

    private Product() { }

    public static Product Create(Guid tenantId, string sku, string barcode, string name,
        string nameBn, Guid categoryId, Money price, Guid vatRateId,
        UnitType unitType = UnitType.Each, bool isWeightBased = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(barcode);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Product
        {
            TenantId = tenantId,
            Sku = sku.ToUpperInvariant(),
            Barcode = barcode,
            Name = name,
            NameBn = nameBn,
            CategoryId = categoryId,
            Price = price,
            VatRateId = vatRateId,
            UnitType = unitType,
            IsWeightBased = isWeightBased
        };
    }

    public void UpdatePrice(Money newPrice)
    {
        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCostPrice(Money? costPrice)
    {
        CostPrice = costPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string nameBn, string? description,
        Guid categoryId, Guid vatRateId, UnitType unitType, string? unitLabel,
        bool isWeightBased, bool isEbtEligible, bool trackInventory,
        int reorderLevel, string? imageUrl, Money? costPrice = null)
    {
        Name = name;
        NameBn = nameBn;
        Description = description;
        CategoryId = categoryId;
        VatRateId = vatRateId;
        UnitType = unitType;
        UnitLabel = unitLabel;
        IsWeightBased = isWeightBased;
        IsEbtEligible = isEbtEligible;
        TrackInventory = trackInventory;
        ReorderLevel = reorderLevel;
        ImageUrl = imageUrl;
        if (costPrice is not null) CostPrice = costPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAgeRestriction(int years)
    {
        IsAgeRestricted = true;
        AgeRestrictionYears = years;
    }

    public void RemoveAgeRestriction()
    {
        IsAgeRestricted = false;
        AgeRestrictionYears = null;
    }

    public void SetPlu(string plu) => Plu = plu;

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
}
