using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class CustomerRepository : TenantRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<Customer?> GetByCodeAsync(Guid tenantId, string code,
        CancellationToken ct = default) =>
        await Db.Customers.FirstOrDefaultAsync(
            c => c.TenantId == tenantId && c.Code == code && !c.IsDeleted, ct);

    public async Task<Customer?> GetByPhoneAsync(Guid tenantId, string phone,
        CancellationToken ct = default) =>
        await Db.Customers.FirstOrDefaultAsync(
            c => c.TenantId == tenantId && c.Phone == phone && !c.IsDeleted, ct);

    public async Task<Customer?> GetByEmailAsync(Guid tenantId, string email,
        CancellationToken ct = default) =>
        await Db.Customers.FirstOrDefaultAsync(
            c => c.TenantId == tenantId && c.Email == email.ToLowerInvariant() && !c.IsDeleted, ct);

    public async Task<IEnumerable<Customer>> SearchAsync(Guid tenantId, string term,
        int limit = 20, CancellationToken ct = default) =>
        await Db.Customers
            .Where(c => c.TenantId == tenantId && !c.IsDeleted &&
                (c.Name.Contains(term) || c.Code.Contains(term) ||
                 (c.Phone != null && c.Phone.Contains(term)) ||
                 (c.Email != null && c.Email.Contains(term))))
            .OrderBy(c => c.Name)
            .Take(limit)
            .ToListAsync(ct);
}
