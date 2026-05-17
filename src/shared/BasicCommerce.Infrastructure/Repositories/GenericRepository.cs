using System.Linq.Expressions;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly BasicCommerceDbContext Db;
    protected readonly DbSet<T> Set;

    public GenericRepository(BasicCommerceDbContext db)
    {
        Db = db;
        Set = db.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await Set.FindAsync([id], ct);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default) =>
        await Set.ToListAsync(ct);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate,
        CancellationToken ct = default) =>
        await Set.Where(predicate).ToListAsync(ct);

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,
        CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(predicate, ct);

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate,
        CancellationToken ct = default) =>
        await Set.AnyAsync(predicate, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await Set.AddAsync(entity, ct);

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default) =>
        await Set.AddRangeAsync(entities, ct);

    public void Update(T entity) => Set.Update(entity);

    public void Remove(T entity) => Set.Remove(entity);

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default) =>
        predicate is null
            ? await Set.CountAsync(ct)
            : await Set.CountAsync(predicate, ct);
}

public class TenantRepository<T> : GenericRepository<T>, ITenantRepository<T>
    where T : TenantEntity
{
    public TenantRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<T?> GetByIdForTenantAsync(Guid tenantId, Guid id,
        CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id, ct);

    public async Task<IEnumerable<T>> GetAllForTenantAsync(Guid tenantId,
        CancellationToken ct = default) =>
        await Set.Where(e => e.TenantId == tenantId).ToListAsync(ct);
}
