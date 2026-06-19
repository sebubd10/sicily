using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Interfaces;

public interface IPromotionRepository : ITenantRepository<Promotion>
{
    Task<IEnumerable<Promotion>> GetActivePromotionsAsync(Guid tenantId, Guid? storeId,
        CancellationToken ct = default);
    Task<Promotion?> GetByCouponCodeAsync(Guid tenantId, string couponCode,
        CancellationToken ct = default);
    Task<IEnumerable<Promotion>> GetPagedAsync(Guid tenantId, PromotionStatus? status,
        int page, int pageSize, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(Guid tenantId, PromotionStatus? status,
        CancellationToken ct = default);
    Task<bool> HasActivePromotionsForStoreAsync(Guid tenantId, Guid storeId, CancellationToken ct = default);
}
