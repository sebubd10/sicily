using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Entities;

public class Terminal : TenantEntity
{
    public Guid StoreId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public TerminalType Type { get; private set; }
    public TerminalStatus Status { get; private set; } = TerminalStatus.Offline;
    public Guid? CurrentCashierId { get; private set; }
    public decimal OpeningFloat { get; private set; }
    public DateTime? LastActivityAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Terminal() { }

    public static Terminal Create(Guid tenantId, Guid storeId, string name,
        string code, TerminalType type = TerminalType.Standard)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        return new Terminal
        {
            TenantId = tenantId,
            StoreId = storeId,
            Name = name,
            Code = code.ToUpperInvariant(),
            Type = type
        };
    }

    public void OpenShift(Guid cashierId, decimal openingFloat)
    {
        CurrentCashierId = cashierId;
        OpeningFloat = openingFloat;
        Status = TerminalStatus.Online;
        LastActivityAt = DateTime.UtcNow;
    }

    public void CloseShift()
    {
        CurrentCashierId = null;
        OpeningFloat = 0;
        Status = TerminalStatus.Offline;
    }

    public void RecordActivity() => LastActivityAt = DateTime.UtcNow;
    public void Deactivate() => IsActive = false;
}
