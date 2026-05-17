using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class CreditAccountRepository : TenantRepository<CreditAccount>, ICreditAccountRepository
{
    public CreditAccountRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<CreditAccount?> GetByCustomerAndStoreAsync(Guid tenantId, Guid customerId,
        Guid storeId, CancellationToken ct = default) =>
        await Db.CreditAccounts.FirstOrDefaultAsync(
            a => a.TenantId == tenantId && a.CustomerId == customerId &&
                 a.StoreId == storeId && a.Status == EntityStatus.Active, ct);

    public async Task<IEnumerable<CreditAccount>> GetByCustomerAsync(Guid tenantId,
        Guid customerId, CancellationToken ct = default) =>
        await Db.CreditAccounts
            .Where(a => a.TenantId == tenantId && a.CustomerId == customerId)
            .OrderByDescending(a => a.OutstandingBalance)
            .ToListAsync(ct);

    public async Task<CreditAccount?> GetWithTransactionsAsync(Guid tenantId,
        Guid creditAccountId, CancellationToken ct = default) =>
        await Db.CreditAccounts
            .Include(a => a.Transactions.OrderByDescending(t => t.CreatedAt).Take(50))
            .FirstOrDefaultAsync(
                a => a.TenantId == tenantId && a.Id == creditAccountId, ct);
}
