using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Entities;

public class Manufacturer : TenantEntity
{
    public string Name { get; private set; } = default!;
    public string? Code { get; private set; }
    public string? Country { get; private set; }
    public string? Website { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? Notes { get; private set; }

    private Manufacturer() { }

    public static Manufacturer Create(Guid tenantId, string name, string? code = null,
        string? country = null, string? website = null, string? contactEmail = null,
        string? notes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Manufacturer
        {
            TenantId = tenantId,
            Name = name,
            Code = code?.ToUpperInvariant(),
            Country = country,
            Website = website,
            ContactEmail = contactEmail,
            Notes = notes
        };
    }

    public void Update(string name, string? code, string? country, string? website,
        string? contactEmail, string? notes)
    {
        Name = name;
        Code = code?.ToUpperInvariant();
        Country = country;
        Website = website;
        ContactEmail = contactEmail;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() { Status = EntityStatus.Inactive; UpdatedAt = DateTime.UtcNow; }
    public void Activate() { Status = EntityStatus.Active; UpdatedAt = DateTime.UtcNow; }
}
