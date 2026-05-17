namespace BasicCommerce.Contracts.Inventory;

public record StockLevelResponse(
    Guid ProductId,
    string ProductName,
    string Sku,
    string Barcode,
    string CategoryName,
    decimal Quantity,
    decimal ReservedQuantity,
    decimal AvailableQuantity,
    decimal LowStockThreshold,
    bool IsLowStock,
    bool IsOutOfStock,
    DateTime? LastCountedAt);

public record StockMovementResponse(
    Guid Id,
    string Type,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    decimal Quantity,
    decimal QuantityBefore,
    decimal QuantityAfter,
    string? Reference,
    string? Notes,
    Guid? RelatedStoreId,
    string? RelatedStoreName,
    Guid RecordedByUserId,
    string RecordedByName,
    DateTime CreatedAt);

public record ReceiveStockRequest(
    Guid ProductId,
    decimal Quantity,
    string? Reference = null,
    string? Notes = null);

public record AdjustStockRequest(
    Guid ProductId,
    decimal NewQuantity,
    string Notes = "Manual adjustment");

public record WriteOffStockRequest(
    Guid ProductId,
    decimal Quantity,
    string Reason);

public record TransferStockRequest(
    Guid SourceStoreId,
    Guid DestinationStoreId,
    Guid ProductId,
    decimal Quantity,
    string? Notes = null);

public record SetLowStockThresholdRequest(
    Guid ProductId,
    decimal Threshold);

public record StockBatchResponse(
    Guid Id,
    Guid StoreId,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    string? LotNumber,
    DateTime? ExpiryDate,
    decimal ReceivedQuantity,
    decimal RemainingQuantity,
    decimal? UnitCost,
    bool IsExpired,
    DateTime CreatedAt);

public record StockBatchListResponse(
    IReadOnlyList<StockBatchResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record ReceiveStockBatchRequest(
    Guid ProductId,
    decimal Quantity,
    DateTime? ExpiryDate = null,
    string? LotNumber = null,
    decimal? UnitCost = null,
    Guid? PurchaseOrderId = null,
    string? Reference = null,
    string? Notes = null);

public record ExpireStockBatchesRequest(
    Guid StoreId,
    string? Notes = null);
