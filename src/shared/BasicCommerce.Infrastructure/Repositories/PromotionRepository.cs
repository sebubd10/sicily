using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class PromotionRepository : TenantRepository<Promotion>, IPromotionRepository
{
    public PromotionRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<IEnumerable<Promotion>> GetActivePromotionsAsync(
        Guid tenantId, Guid? storeId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var q = Set.Where(p => p.TenantId == tenantId
            && p.PromotionStatus == PromotionStatus.Active
            && (p.StartsAt == null || p.StartsAt <= now)
            && (p.EndsAt == null || p.EndsAt >= now)
            && (p.MaxUses == null || p.UsedCount < p.MaxUses));
        if (storeId.HasValue)
            q = q.Where(p => p.StoreId == null || p.StoreId == storeId.Value);
        return await q.ToListAsync(ct);
    }

    public async Task<Promotion?> GetByCouponCodeAsync(Guid tenantId, string couponCode,
        CancellationToken ct = default)
        => await Set
            .FirstOrDefaultAsync(p => p.TenantId == tenantId
                && p.CouponCode == couponCode.Trim().ToUpperInvariant(), ct);

    public async Task<IEnumerable<Promotion>> GetPagedAsync(Guid tenantId,
        PromotionStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var q = Set.Where(p => p.TenantId == tenantId);
        if (status.HasValue) q = q.Where(p => p.PromotionStatus == status.Value);
        return await q.OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
    }

    public async Task<int> GetTotalCountAsync(Guid tenantId, PromotionStatus? status,
        CancellationToken ct = default)
    {
        var q = Set.Where(p => p.TenantId == tenantId);
        if (status.HasValue) q = q.Where(p => p.PromotionStatus == status.Value);
        return await q.CountAsync(ct);
    }
}
