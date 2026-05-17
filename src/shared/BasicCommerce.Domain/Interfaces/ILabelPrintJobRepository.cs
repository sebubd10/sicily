using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Interfaces;

public interface ILabelPrintJobRepository : ITenantRepository<LabelPrintJob>
{
    Task<LabelPrintJob?> GetWithItemsAsync(Guid tenantId, Guid id,
        CancellationToken ct = default);
    Task<IEnumerable<LabelPrintJob>> GetPagedAsync(Guid tenantId, Guid? storeId,
        LabelPrintJobStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(Guid tenantId, Guid? storeId,
        LabelPrintJobStatus? status, CancellationToken ct = default);
}
