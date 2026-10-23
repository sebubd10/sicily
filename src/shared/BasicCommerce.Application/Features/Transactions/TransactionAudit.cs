using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;

namespace BasicCommerce.Application.Features.Transactions;

/// <summary>Records an entry in a transaction's audit history (saved with the caller's SaveChanges).</summary>
internal static class TransactionAudit
{
    public static Task RecordAsync(IUnitOfWork uow, Transaction transaction,
        TransactionAuditAction action, string summary, Guid? userId,
        string? reason = null, CancellationToken ct = default) =>
        uow.TransactionAudits.AddAsync(TransactionAuditEntry.Create(
            transaction.TenantId, transaction.Id, action, summary,
            userId == Guid.Empty ? null : userId, reason), ct);

    public static string Money(decimal amount) => $"৳{amount:N2}";
}
