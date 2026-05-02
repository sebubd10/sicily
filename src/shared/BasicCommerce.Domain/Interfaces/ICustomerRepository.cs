using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface ICustomerRepository : ITenantRepository<Customer>
{
    Task<Customer?> GetByCodeAsync(Guid tenantId, string code, CancellationToken ct = default);
    Task<Customer?> GetByPhoneAsync(Guid tenantId, string phone, CancellationToken ct = default);
    Task<Customer?> GetByEmailAsync(Guid tenantId, string email, CancellationToken ct = default);
    Task<IEnumerable<Customer>> SearchAsync(Guid tenantId, string term, int limit = 20, CancellationToken ct = default);
}
