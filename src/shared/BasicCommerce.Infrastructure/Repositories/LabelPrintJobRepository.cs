using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class LabelPrintJobRepository : TenantRepository<LabelPrintJob>, ILabelPrintJobRepository
{
    public LabelPrintJobRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<LabelPrintJob?> GetWithItemsAsync(Guid tenantId, Guid id,
        CancellationToken ct = default)
        => await Set
            .Include(j => j.Items)
            .FirstOrDefaultAsync(j => j.TenantId == tenantId && j.Id == id, ct);

    public async Task<IEnumerable<LabelPrintJob>> GetPagedAsync(Guid tenantId, Guid? storeId,
        LabelPrintJobStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var q = Set.Where(j => j.TenantId == tenantId);
        if (storeId.HasValue) q = q.Where(j => j.StoreId == storeId.Value);
        if (status.HasValue) q = q.Where(j => j.JobStatus == status.Value);
        return await q.OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
    }

    public async Task<int> GetTotalCountAsync(Guid tenantId, Guid? storeId,
        LabelPrintJobStatus? status, CancellationToken ct = default)
    {
        var q = Set.Where(j => j.TenantId == tenantId);
        if (storeId.HasValue) q = q.Where(j => j.StoreId == storeId.Value);
        if (status.HasValue) q = q.Where(j => j.JobStatus == status.Value);
        return await q.CountAsync(ct);
    }
}
