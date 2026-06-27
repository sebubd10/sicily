using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class CustomerRepository : TenantRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(BasicCommerceDbContext db) : base(db) { }

    private IQueryable<Customer> ActiveSet =>
        Db.Customers.Where(c => c.Status != EntityStatus.Deleted);

    public async Task<Customer?> GetByCodeAsync(Guid tenantId, string code,
        CancellationToken ct = default) =>
        await ActiveSet.FirstOrDefaultAsync(
            c => c.TenantId == tenantId && c.Code == code, ct);

    public async Task<Customer?> GetByPhoneAsync(Guid tenantId, string phone,
        CancellationToken ct = default) =>
        await ActiveSet.FirstOrDefaultAsync(
            c => c.TenantId == tenantId && c.Phone == phone, ct);

    public async Task<Customer?> GetByEmailAsync(Guid tenantId, string email,
        CancellationToken ct = default) =>
        await ActiveSet.FirstOrDefaultAsync(
            c => c.TenantId == tenantId && c.Email == email.ToLowerInvariant(), ct);

    public async Task<IEnumerable<Customer>> SearchAsync(Guid tenantId, string term,
        int limit = 20, CancellationToken ct = default) =>
        await ActiveSet
            .Where(c => c.TenantId == tenantId &&
                (c.Name.Contains(term) || c.Code.Contains(term) ||
                 (c.Phone != null && c.Phone.Contains(term)) ||
                 (c.Email != null && c.Email.Contains(term))))
            .OrderBy(c => c.Name)
            .Take(limit)
            .ToListAsync(ct);

    public async Task<int> GetCountAsync(Guid tenantId, string? term = null,
        CancellationToken ct = default)
    {
        var query = ActiveSet.Where(c => c.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(c =>
                c.Name.Contains(term) || c.Code.Contains(term) ||
                (c.Phone != null && c.Phone.Contains(term)) ||
                (c.Email != null && c.Email.Contains(term)));
        return await query.CountAsync(ct);
    }

    public async Task<IEnumerable<Customer>> GetPagedAsync(Guid tenantId, string? term,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = ActiveSet.Where(c => c.TenantId == tenantId).AsQueryable();
        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(c =>
                c.Name.Contains(term) || c.Code.Contains(term) ||
                (c.Phone != null && c.Phone.Contains(term)) ||
                (c.Email != null && c.Email.Contains(term)));
        return await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }
}

