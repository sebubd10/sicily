using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string ContactEmail { get; private set; } = default!;
    public string? ContactPhone { get; private set; }
    public string? BusinessIdentificationNumber { get; private set; }
    public string? VatRegistrationNumber { get; private set; }
    public string CurrencyCode { get; private set; } = "BDT";
    public string DefaultLanguage { get; private set; } = "en";


    private readonly List<Store> _stores = [];
    public IReadOnlyCollection<Store> Stores => _stores.AsReadOnly();

    private Tenant() { }

    public static Tenant Create(string name, string slug, string contactEmail,
        string? bin = null, string? vatRegistration = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(contactEmail);

        return new Tenant
        {
            Name = name,
            Slug = slug.ToLowerInvariant(),
            ContactEmail = contactEmail,
            BusinessIdentificationNumber = bin,
            VatRegistrationNumber = vatRegistration
        };
    }

    public void Update(string name, string contactEmail, string? phone,
        string? bin, string? vatRegistration)
    {
        Name = name;
        ContactEmail = contactEmail;
        ContactPhone = phone;
        BusinessIdentificationNumber = bin;
        VatRegistrationNumber = vatRegistration;
        UpdatedAt = DateTime.UtcNow;
    }

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
