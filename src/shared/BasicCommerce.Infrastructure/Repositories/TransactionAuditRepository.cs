using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class TransactionAuditRepository : ITransactionAuditRepository
{
    private readonly BasicCommerceDbContext _db;

    public TransactionAuditRepository(BasicCommerceDbContext db) => _db = db;

    public async Task AddAsync(TransactionAuditEntry entry, CancellationToken ct = default) =>
        await _db.TransactionAuditEntries.AddAsync(entry, ct);

    public async Task<IReadOnlyList<TransactionAuditEntry>> GetByTransactionAsync(Guid tenantId,
        Guid transactionId, CancellationToken ct = default) =>
        await _db.TransactionAuditEntries
            .Where(e => e.TenantId == tenantId && e.TransactionId == transactionId)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(ct);
}
