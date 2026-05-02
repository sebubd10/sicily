using BasicCommerce.Domain.ValueObjects;

namespace BasicCommerce.Domain.Entities;

public class Store : TenantEntity
{
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public Address Address { get; private set; } = default!;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public bool IsActive { get; private set; } = true;
    public TimeOnly OpeningTime { get; private set; }
    public TimeOnly ClosingTime { get; private set; }

    private readonly List<Terminal> _terminals = [];
    public IReadOnlyCollection<Terminal> Terminals => _terminals.AsReadOnly();

    private Store() { }

    public static Store Create(Guid tenantId, string name, string code, Address address,
        string? phone = null, string? email = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        return new Store
        {
            TenantId = tenantId,
            Name = name,
            Code = code.ToUpperInvariant(),
            Address = address,
            Phone = phone,
            Email = email,
            OpeningTime = new TimeOnly(8, 0),
            ClosingTime = new TimeOnly(22, 0)
        };
    }

    public void Update(string name, Address address, string? phone, string? email,
        TimeOnly openingTime, TimeOnly closingTime)
    {
        Name = name;
        Address = address;
        Phone = phone;
        Email = email;
        OpeningTime = openingTime;
        ClosingTime = closingTime;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
