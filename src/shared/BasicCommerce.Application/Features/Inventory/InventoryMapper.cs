using BasicCommerce.Contracts.Inventory;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;

namespace BasicCommerce.Application.Features.Inventory;

internal static class InventoryMapper
{
    internal static StockLevelResponse ToResponse(StockLevel level, Product product, string categoryName) =>
        new(product.Id,
            product.Name,
            product.Sku,
            product.Barcode,
            categoryName,
            level.Quantity,
            level.ReservedQuantity,
            level.AvailableQuantity,
            level.LowStockThreshold,
            level.IsLowStock,
            level.Quantity <= 0,
            level.LastCountedAt);

    internal static async Task<StockLevelResponse> ToResponseWithCategoryAsync(
        StockLevel level, Product product, IUnitOfWork uow,
        Dictionary<Guid, string> cache, CancellationToken ct)
    {
        if (!cache.TryGetValue(product.CategoryId, out var catName))
        {
            var cat = await uow.Categories.GetByIdAsync(product.CategoryId, ct);
            catName = cat?.Name ?? string.Empty;
            cache[product.CategoryId] = catName;
        }
        return ToResponse(level, product, catName);
    }
}
