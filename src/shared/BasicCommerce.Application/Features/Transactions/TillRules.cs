using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;

namespace BasicCommerce.Application.Features.Transactions;

internal static class TillRules
{
    /// <summary>
    /// Money-affecting corrections to a completed sale (void, payment-method fix) are only
    /// allowed while the till session that took the sale is still open, so the shift's cash
    /// count stays right. After that, use a return.
    /// </summary>
    public static async Task EnsureSaleInOpenTillSessionAsync(IUnitOfWork uow, Transaction transaction,
        string action, CancellationToken ct)
    {
        var session = await uow.TillSessions.GetOpenSessionAsync(transaction.TenantId, transaction.TerminalId, ct);
        var saleTime = transaction.CompletedAt ?? transaction.CreatedAt;

        if (session is null || saleTime < session.OpenedAt)
            throw new DomainException(
                $"This sale can only be {action} while the till session it was rung in is still open. " +
                "Use a return instead.");
    }
}
