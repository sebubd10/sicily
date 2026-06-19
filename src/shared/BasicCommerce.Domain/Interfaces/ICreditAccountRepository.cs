using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Domain.Interfaces;

public interface ICreditAccountRepository : ITenantRepository<CreditAccount>
{
    Task<CreditAccount?> GetByCustomerAndStoreAsync(Guid tenantId, Guid customerId, Guid storeId,
        CancellationToken ct = default);
    Task<IEnumerable<CreditAccount>> GetByCustomerAsync(Guid tenantId, Guid customerId,
        CancellationToken ct = default);
    Task<CreditAccount?> GetWithTransactionsAsync(Guid tenantId, Guid creditAccountId,
        CancellationToken ct = default);
    Task<bool> HasOutstandingCreditForStoreAsync(Guid tenantId, Guid storeId, CancellationToken ct = default);
}
