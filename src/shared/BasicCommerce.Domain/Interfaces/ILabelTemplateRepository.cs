using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Interfaces;

public interface ILabelTemplateRepository : ITenantRepository<LabelTemplate>
{
    Task<IEnumerable<LabelTemplate>> GetPagedAsync(Guid tenantId, LabelType? labelType,
        int page, int pageSize, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(Guid tenantId, LabelType? labelType,
        CancellationToken ct = default);
    Task<LabelTemplate?> GetDefaultForTypeAsync(Guid tenantId, LabelType labelType,
        CancellationToken ct = default);
    Task ClearDefaultForTypeAsync(Guid tenantId, LabelType labelType,
        CancellationToken ct = default);
}
