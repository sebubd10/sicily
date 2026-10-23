using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Entities;

/// <summary>
/// Append-only history of everything done to a transaction: who, when, what and why.
/// </summary>
public class TransactionAuditEntry : TenantEntity
{
    public Guid TransactionId { get; private set; }
    public TransactionAuditAction Action { get; private set; }
    /// <summary>Human-readable description, e.g. "Qty of Acme Energy Drink changed 1 → 2".</summary>
    public string Summary { get; private set; } = default!;
    public string? Reason { get; private set; }
    public Guid? UserId { get; private set; }

    private TransactionAuditEntry() { }

    public static TransactionAuditEntry Create(Guid tenantId, Guid transactionId,
        TransactionAuditAction action, string summary, Guid? userId, string? reason = null) =>
        new()
        {
            TenantId = tenantId,
            TransactionId = transactionId,
            Action = action,
            Summary = summary.Length > 1000 ? summary[..1000] : summary,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            UserId = userId,
            CreatedBy = userId,
        };
}
