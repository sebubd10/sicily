using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class StoreRepository : TenantRepository<Store>, IStoreRepository
{
    public StoreRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<Store?> GetByCodeAsync(Guid tenantId, string code,
        CancellationToken ct = default) =>
        await Db.Stores.FirstOrDefaultAsync(
            s => s.TenantId == tenantId && s.Code == code.ToUpperInvariant(), ct);

    public async Task<bool> CodeExistsAsync(Guid tenantId, string code,
        CancellationToken ct = default) =>
        await Db.Stores.AnyAsync(
            s => s.TenantId == tenantId && s.Code == code.ToUpperInvariant(), ct);
}

public class TerminalRepository : TenantRepository<Terminal>, ITerminalRepository
{
    public TerminalRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<IEnumerable<Terminal>> GetByStoreAsync(Guid tenantId, Guid storeId,
        CancellationToken ct = default) =>
        await Db.Terminals
            .Where(t => t.TenantId == tenantId && t.StoreId == storeId)
            .OrderBy(t => t.Code)
            .ToListAsync(ct);

    public async Task<Terminal?> GetByCodeAsync(Guid tenantId, Guid storeId, string code,
        CancellationToken ct = default) =>
        await Db.Terminals.FirstOrDefaultAsync(
            t => t.TenantId == tenantId && t.StoreId == storeId &&
                 t.Code == code.ToUpperInvariant(), ct);

    public async Task<bool> CodeExistsAsync(Guid tenantId, Guid storeId, string code,
        Guid? excludeId = null, CancellationToken ct = default) =>
        await Db.Terminals.AnyAsync(
            t => t.TenantId == tenantId && t.StoreId == storeId &&
                 t.Code == code.ToUpperInvariant() &&
                 (excludeId == null || t.Id != excludeId), ct);
}
