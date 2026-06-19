using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class RewardPointsSettingsRepository
    : TenantRepository<RewardPointsSettings>, IRewardPointsSettingsRepository
{
    public RewardPointsSettingsRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<RewardPointsSettings?> GetByTenantAsync(Guid tenantId,
        CancellationToken ct = default) =>
        await Db.RewardPointsSettings.FirstOrDefaultAsync(r => r.TenantId == tenantId, ct);
}

public class RewardPointsAccountRepository
    : TenantRepository<RewardPointsAccount>, IRewardPointsAccountRepository
{
    public RewardPointsAccountRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<RewardPointsAccount?> GetByCustomerAsync(Guid tenantId, Guid customerId,
        Guid? storeId = null, CancellationToken ct = default) =>
        await Db.RewardPointsAccounts.FirstOrDefaultAsync(r =>
            r.TenantId == tenantId && r.CustomerId == customerId && r.StoreId == storeId, ct);

    public async Task<RewardPointsAccount?> GetWithEntriesAsync(Guid tenantId, Guid customerId,
        Guid? storeId = null, CancellationToken ct = default) =>
        await Db.RewardPointsAccounts
            .Include(r => r.Entries.OrderByDescending(e => e.CreatedAt).Take(100))
            .FirstOrDefaultAsync(r =>
                r.TenantId == tenantId && r.CustomerId == customerId && r.StoreId == storeId, ct);

    public async Task<bool> HasPointsForStoreAsync(Guid tenantId, Guid storeId,
        CancellationToken ct = default) =>
        await Db.RewardPointsAccounts.AnyAsync(
            r => r.TenantId == tenantId && r.StoreId == storeId
              && (r.TotalEarnedPoints - r.UsedPoints - r.ExpiredPoints) > 0, ct);
}
