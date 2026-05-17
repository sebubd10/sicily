using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class TillSessionRepository : TenantRepository<TillSession>, ITillSessionRepository
{
    public TillSessionRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<TillSession?> GetWithPettyAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        => await Set
            .Include(t => t.PettyTransactions)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, ct);

    public async Task<TillSession?> GetOpenSessionAsync(Guid tenantId, Guid terminalId, CancellationToken ct = default)
        => await Set
            .Include(t => t.PettyTransactions)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId
                && t.TerminalId == terminalId
                && t.SessionStatus == TillSessionStatus.Open, ct);

    public async Task<IEnumerable<TillSession>> GetPagedAsync(Guid tenantId, Guid? storeId,
        Guid? terminalId, bool openOnly, int page, int pageSize, CancellationToken ct = default)
    {
        var q = Set.Where(t => t.TenantId == tenantId);
        if (storeId.HasValue) q = q.Where(t => t.StoreId == storeId.Value);
        if (terminalId.HasValue) q = q.Where(t => t.TerminalId == terminalId.Value);
        if (openOnly) q = q.Where(t => t.SessionStatus == TillSessionStatus.Open);
        return await q.OrderByDescending(t => t.OpenedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
    }

    public async Task<int> GetTotalCountAsync(Guid tenantId, Guid? storeId,
        Guid? terminalId, bool openOnly, CancellationToken ct = default)
    {
        var q = Set.Where(t => t.TenantId == tenantId);
        if (storeId.HasValue) q = q.Where(t => t.StoreId == storeId.Value);
        if (terminalId.HasValue) q = q.Where(t => t.TerminalId == terminalId.Value);
        if (openOnly) q = q.Where(t => t.SessionStatus == TillSessionStatus.Open);
        return await q.CountAsync(ct);
    }
}
