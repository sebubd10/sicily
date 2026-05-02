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
