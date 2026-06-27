using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
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

    public async Task<IEnumerable<Transaction>> SearchPagedAsync(
        Guid tenantId, string? term, Guid? storeId,
        TransactionStatus? status, TransactionType? type,
        DateTime? from, DateTime? to, Guid? customerId,
        int page, int pageSize, CancellationToken ct = default)
    {
        var q = BuildQuery(tenantId, term, storeId, status, type, from, to, customerId);
        return await q
            .Include(t => t.LineItems)
            .Include(t => t.Payments)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> SearchCountAsync(
        Guid tenantId, string? term, Guid? storeId,
        TransactionStatus? status, TransactionType? type,
        DateTime? from, DateTime? to, Guid? customerId,
        CancellationToken ct = default) =>
        await BuildQuery(tenantId, term, storeId, status, type, from, to, customerId)
            .CountAsync(ct);

    private IQueryable<Transaction> BuildQuery(
        Guid tenantId, string? term, Guid? storeId,
        TransactionStatus? status, TransactionType? type,
        DateTime? from, DateTime? to, Guid? customerId)
    {
        var q = Db.Transactions.Where(t => t.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(term))
            q = q.Where(t => t.TransactionNumber.Contains(term));
        if (storeId.HasValue)    q = q.Where(t => t.StoreId == storeId.Value);
        if (status.HasValue)     q = q.Where(t => t.TransactionStatus == status.Value);
        if (type.HasValue)       q = q.Where(t => t.Type == type.Value);
        if (from.HasValue)       q = q.Where(t => t.CreatedAt >= from.Value);
        if (to.HasValue)         q = q.Where(t => t.CreatedAt <= to.Value);
        if (customerId.HasValue) q = q.Where(t => t.CustomerId == customerId.Value);
        return q;
    }
}
