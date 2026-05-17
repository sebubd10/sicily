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

    public async Task<IEnumerable<Transaction>> GetForDailyReportAsync(Guid tenantId,
        Guid? storeId, DateOnly date, CancellationToken ct = default)
    {
        var from = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var to = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        return await Db.Transactions
            .Include(t => t.Payments)
            .Include(t => t.LineItems)
            .Where(t => t.TenantId == tenantId &&
                (storeId == null || t.StoreId == storeId) &&
                t.CreatedAt >= from && t.CreatedAt <= to)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Transaction>> GetForDateRangeReportAsync(Guid tenantId,
        Guid? storeId, DateTime from, DateTime to, CancellationToken ct = default) =>
        await Db.Transactions
            .Include(t => t.Payments)
            .Include(t => t.LineItems)
            .Where(t => t.TenantId == tenantId &&
                (storeId == null || t.StoreId == storeId) &&
                t.CreatedAt >= from && t.CreatedAt <= to)
            .ToListAsync(ct);

    public async Task<IEnumerable<Transaction>> GetForReconciliationAsync(Guid tenantId,
        Guid terminalId, DateOnly date, CancellationToken ct = default)
    {
        var from = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var to = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        return await Db.Transactions
            .Include(t => t.Payments)
            .Where(t => t.TenantId == tenantId && t.TerminalId == terminalId &&
                t.CreatedAt >= from && t.CreatedAt <= to)
            .ToListAsync(ct);
    }
}
