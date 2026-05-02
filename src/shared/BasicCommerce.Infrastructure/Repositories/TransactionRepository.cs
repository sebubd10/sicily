using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class TransactionRepository : TenantRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<Transaction?> GetByNumberAsync(Guid tenantId, string transactionNumber,
        CancellationToken ct = default) =>
        await Db.Transactions
            .Include(t => t.LineItems)
            .Include(t => t.Payments)
            .FirstOrDefaultAsync(
                t => t.TenantId == tenantId && t.TransactionNumber == transactionNumber, ct);

    public async Task<Transaction?> GetWithItemsAsync(Guid tenantId, Guid transactionId,
        CancellationToken ct = default) =>
        await Db.Transactions
            .Include(t => t.LineItems)
            .Include(t => t.Payments)
            .FirstOrDefaultAsync(
                t => t.TenantId == tenantId && t.Id == transactionId, ct);

    public async Task<IEnumerable<Transaction>> GetByTerminalAsync(Guid tenantId,
        Guid terminalId, DateTime from, DateTime to, CancellationToken ct = default) =>
        await Db.Transactions
            .Where(t => t.TenantId == tenantId && t.TerminalId == terminalId &&
                t.CreatedAt >= from && t.CreatedAt <= to)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
}
