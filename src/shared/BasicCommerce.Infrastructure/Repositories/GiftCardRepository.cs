using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class GiftCardRepository : TenantRepository<GiftCard>, IGiftCardRepository
{
    public GiftCardRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<GiftCard?> GetByCodeAsync(Guid tenantId, string code, CancellationToken ct = default)
        => await Set
            .FirstOrDefaultAsync(g => g.TenantId == tenantId
                && g.Code == code.Trim().ToUpperInvariant(), ct);

    public async Task<GiftCard?> GetWithTransactionsAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        => await Set
            .Include(g => g.Transactions)
            .FirstOrDefaultAsync(g => g.TenantId == tenantId && g.Id == id, ct);

    public async Task<IEnumerable<GiftCard>> GetPagedAsync(Guid tenantId, Guid? storeId,
        string? status, int page, int pageSize, CancellationToken ct = default)
    {
        var q = Set.Where(g => g.TenantId == tenantId);
        if (storeId.HasValue) q = q.Where(g => g.StoreId == storeId.Value);
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<GiftCardStatus>(status, true, out var s))
            q = q.Where(g => g.CardStatus == s);
        return await q.OrderByDescending(g => g.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
    }

    public async Task<int> GetTotalCountAsync(Guid tenantId, Guid? storeId,
        string? status, CancellationToken ct = default)
    {
        var q = Set.Where(g => g.TenantId == tenantId);
        if (storeId.HasValue) q = q.Where(g => g.StoreId == storeId.Value);
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<GiftCardStatus>(status, true, out var s))
            q = q.Where(g => g.CardStatus == s);
        return await q.CountAsync(ct);
    }

    public async Task<IEnumerable<GiftCard>> GetExpiredUnprocessedAsync(Guid tenantId,
        CancellationToken ct = default)
        => await Set
            .Where(g => g.TenantId == tenantId
                && g.CardStatus == GiftCardStatus.Active
                && g.ExpiryDate.HasValue
                && g.ExpiryDate.Value < DateTime.UtcNow.Date)
            .ToListAsync(ct);
}
