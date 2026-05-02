using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface IVatRateRepository : ITenantRepository<VatRate>
{
    Task<VatRate?> GetDefaultAsync(Guid tenantId, CancellationToken ct = default);
}
