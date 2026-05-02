using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface ITransactionRepository : ITenantRepository<Transaction>
{
    Task<Transaction?> GetByNumberAsync(Guid tenantId, string transactionNumber, CancellationToken ct = default);
    Task<Transaction?> GetWithItemsAsync(Guid tenantId, Guid transactionId, CancellationToken ct = default);
    Task<IEnumerable<Transaction>> GetByTerminalAsync(Guid tenantId, Guid terminalId,
        DateTime from, DateTime to, CancellationToken ct = default);
}
