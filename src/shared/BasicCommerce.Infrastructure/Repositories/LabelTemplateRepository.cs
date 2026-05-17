using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using BasicCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicCommerce.Infrastructure.Repositories;

public class LabelTemplateRepository : TenantRepository<LabelTemplate>, ILabelTemplateRepository
{
    public LabelTemplateRepository(BasicCommerceDbContext db) : base(db) { }

    public async Task<IEnumerable<LabelTemplate>> GetPagedAsync(Guid tenantId,
        LabelType? labelType, int page, int pageSize, CancellationToken ct = default)
    {
        var q = Set.Where(t => t.TenantId == tenantId);
        if (labelType.HasValue) q = q.Where(t => t.LabelType == labelType.Value);
        return await q.OrderBy(t => t.SortOrder).ThenBy(t => t.Name)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
    }

    public async Task<int> GetTotalCountAsync(Guid tenantId, LabelType? labelType,
        CancellationToken ct = default)
    {
        var q = Set.Where(t => t.TenantId == tenantId);
        if (labelType.HasValue) q = q.Where(t => t.LabelType == labelType.Value);
        return await q.CountAsync(ct);
    }

    public async Task<LabelTemplate?> GetDefaultForTypeAsync(Guid tenantId,
        LabelType labelType, CancellationToken ct = default)
        => await Set.FirstOrDefaultAsync(t => t.TenantId == tenantId
            && t.LabelType == labelType && t.IsDefault, ct);

    public async Task ClearDefaultForTypeAsync(Guid tenantId, LabelType labelType,
        CancellationToken ct = default)
    {
        var defaults = await Set
            .Where(t => t.TenantId == tenantId && t.LabelType == labelType && t.IsDefault)
            .ToListAsync(ct);
        foreach (var t in defaults)
        {
            t.ClearDefault();
            Set.Update(t);
        }
    }
}
