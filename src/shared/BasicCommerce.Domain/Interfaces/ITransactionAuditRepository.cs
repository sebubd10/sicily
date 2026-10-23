using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface ITransactionAuditRepository
{
    Task AddAsync(TransactionAuditEntry entry, CancellationToken ct = default);
    Task<IReadOnlyList<TransactionAuditEntry>> GetByTransactionAsync(Guid tenantId, Guid transactionId,
        CancellationToken ct = default);
}
