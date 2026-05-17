using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Interfaces;

public interface ISupplierReturnRepository : ITenantRepository<SupplierReturn>
{
    Task<SupplierReturn?> GetWithItemsAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    Task<IEnumerable<SupplierReturn>> GetPagedAsync(
        Guid tenantId, Guid? supplierId, Guid? storeId,
        SupplierReturnStatus? status, int page, int pageSize, CancellationToken ct = default);

    Task<int> GetTotalCountAsync(
        Guid tenantId, Guid? supplierId, Guid? storeId,
        SupplierReturnStatus? status, CancellationToken ct = default);
}
