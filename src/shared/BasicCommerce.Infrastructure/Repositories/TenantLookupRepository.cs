using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class TenantLookupRepository : ITenantLookupRepository
{
    private readonly BasicCommerceDbContext _db;

    public TenantLookupRepository(BasicCommerceDbContext db) => _db = db;

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => await _db.Tenants
            .FirstOrDefaultAsync(t => t.Slug == slug.ToLowerInvariant(), ct);

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Tenants.FindAsync([id], ct);
}
