using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class VatRateRepository : TenantRepository<VatRate>, IVatRateRepository
{
    public VatRateRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<VatRate?> GetDefaultAsync(Guid tenantId, CancellationToken ct = default) =>
        await Db.VatRates.FirstOrDefaultAsync(
            v => v.TenantId == tenantId && v.IsDefault && v.IsActive, ct);
}
