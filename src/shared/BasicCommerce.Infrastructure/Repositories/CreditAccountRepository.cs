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

    public async Task<bool> HasOutstandingCreditForStoreAsync(Guid tenantId, Guid storeId,
        CancellationToken ct = default) =>
        await Db.CreditAccounts.AnyAsync(
            a => a.TenantId == tenantId && a.StoreId == storeId && a.OutstandingBalance > 0, ct);

    public async Task<(IEnumerable<CreditAccount> Items, int TotalCount, decimal TotalOutstanding, decimal TotalCreditExtended)>
        GetPagedAsync(Guid tenantId, string? term, bool? hasBalance, int page, int pageSize, CancellationToken ct = default)
    {
        var query = Db.CreditAccounts.Where(a => a.TenantId == tenantId).AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
        {
            var customerIds = await Db.Customers
                .Where(c => c.TenantId == tenantId &&
                    (c.Name.Contains(term) || c.Code.Contains(term) ||
                     (c.Phone != null && c.Phone.Contains(term))))
                .Select(c => c.Id)
                .ToListAsync(ct);
            query = query.Where(a => customerIds.Contains(a.CustomerId));
        }

        if (hasBalance == true)
            query = query.Where(a => a.OutstandingBalance > 0);
        else if (hasBalance == false)
            query = query.Where(a => a.OutstandingBalance == 0);

        var totalCount      = await query.CountAsync(ct);
        var totalOutstanding = totalCount > 0 ? await query.SumAsync(a => a.OutstandingBalance, ct) : 0m;
        var totalCredit      = totalCount > 0 ? await query.SumAsync(a => a.CreditLimit, ct) : 0m;

        var items = await query
            .OrderByDescending(a => a.OutstandingBalance)
            .ThenBy(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount, totalOutstanding, totalCredit);
    }
}
