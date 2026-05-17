using BasicCommerce.Contracts.Inventory;
using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Application.Features.Inventory;

internal static class BatchMapper
{
    internal static StockBatchResponse ToResponse(StockBatch batch, Product product) =>
        new(batch.Id,
            batch.StoreId,
            batch.ProductId,
            product.Name,
            product.Sku,
            batch.LotNumber,
            batch.ExpiryDate,
            batch.ReceivedQuantity,
            batch.RemainingQuantity,
            batch.UnitCost,
            batch.IsExpired,
            batch.CreatedAt);
}
