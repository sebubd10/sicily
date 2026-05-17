using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.ValueObjects;

namespace BasicCommerce.Domain.Entities;

public class Supplier : TenantEntity
{
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public string? ContactName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public Address? Address { get; private set; }
    public int LeadTimeDays { get; private set; }
    public string? Notes { get; private set; }
    public Guid? ManufacturerId { get; private set; }

    public Manufacturer? Manufacturer { get; private set; }

    private Supplier() { }

    public static Supplier Create(Guid tenantId, string name, string code,
        string? contactName = null, string? email = null, string? phone = null,
        Address? address = null, int leadTimeDays = 0,
        string? notes = null, Guid? manufacturerId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return new Supplier
        {
            TenantId = tenantId,
            Name = name,
            Code = code.ToUpperInvariant(),
            ContactName = contactName,
            Email = email,
            Phone = phone,
            Address = address,
            LeadTimeDays = leadTimeDays,
            Notes = notes,
            ManufacturerId = manufacturerId
        };
    }

    public void Update(string name, string? contactName, string? email, string? phone,
        Address? address, int leadTimeDays, string? notes, Guid? manufacturerId)
    {
        Name = name;
        ContactName = contactName;
        Email = email;
        Phone = phone;
        Address = address;
        LeadTimeDays = leadTimeDays;
        Notes = notes;
        ManufacturerId = manufacturerId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() { Status = EntityStatus.Inactive; UpdatedAt = DateTime.UtcNow; }
    public void Activate() { Status = EntityStatus.Active; UpdatedAt = DateTime.UtcNow; }
}
