using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.ValueObjects;

namespace BasicCommerce.Domain.Entities;

public class Warehouse : TenantEntity
{
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public Address Address { get; private set; } = default!;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public bool IsDefault { get; private set; }

    private Warehouse() { }

    public static Warehouse Create(Guid tenantId, string name, string code, Address address,
        string? phone = null, string? email = null, bool isDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return new Warehouse
        {
            TenantId = tenantId,
            Name = name,
            Code = code.ToUpperInvariant(),
            Address = address,
            Phone = phone,
            Email = email,
            IsDefault = isDefault
        };
    }

    public void Update(string name, Address address, string? phone, string? email)
    {
        Name = name;
        Address = address;
        Phone = phone;
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDefault() { IsDefault = true; UpdatedAt = DateTime.UtcNow; }

    public void Deactivate() { Status = EntityStatus.Inactive; UpdatedAt = DateTime.UtcNow; }
    public void Activate() { Status = EntityStatus.Active; UpdatedAt = DateTime.UtcNow; }
}
