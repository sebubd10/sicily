using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Interfaces;

public interface IProductRepository : ITenantRepository<Product>
{
    Task<Product?> GetByBarcodeAsync(Guid tenantId, string barcode, CancellationToken ct = default);
    Task<Product?> GetByPluAsync(Guid tenantId, string plu, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(Guid tenantId, string sku, CancellationToken ct = default);
    Task<Product?> GetWithTagsAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<IEnumerable<Product>> SearchAsync(Guid tenantId, string term, int limit = 20, CancellationToken ct = default);
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(Guid tenantId,
        int page, int pageSize, Guid? categoryId = null, EntityStatus? status = null,
        string? search = null, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetAllForTenantAsync(Guid tenantId,
        Guid? categoryId = null, bool includeInactive = false,
        string? search = null, CancellationToken ct = default);
}

public interface IStockLevelRepository : ITenantRepository<StockLevel>
{
    Task<StockLevel?> GetAsync(Guid tenantId, Guid storeId, Guid productId, CancellationToken ct = default);
    Task<IEnumerable<StockLevel>> GetByStoreAsync(Guid tenantId, Guid storeId, CancellationToken ct = default);
    Task<IEnumerable<StockLevel>> GetLowStockAsync(Guid tenantId, Guid storeId, CancellationToken ct = default);
}
