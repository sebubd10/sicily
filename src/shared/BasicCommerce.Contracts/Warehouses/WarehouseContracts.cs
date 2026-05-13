namespace BasicCommerce.Contracts.Warehouses;

public record WarehouseResponse(
    Guid Id,
    string Name,
    string Code,
    string? Phone,
    string? Email,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string District,
    string PostalCode,
    string Country,
    bool IsDefault,
    string Status);

public record WarehouseStockLevelResponse(
    Guid WarehouseId,
    string WarehouseName,
    Guid ProductId,
    string ProductName,
    string Sku,
    string Barcode,
    decimal Quantity,
    decimal ReservedQuantity,
    decimal AvailableQuantity,
    decimal LowStockThreshold,
    bool IsLowStock);

public record WarehouseMovementResponse(
    Guid Id,
    string MovementType,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    decimal Quantity,
    decimal QuantityBefore,
    decimal QuantityAfter,
    Guid? RelatedStoreId,
    Guid? PurchaseOrderId,
    string? Reference,
    string? Notes,
    Guid RecordedByUserId,
    DateTime CreatedAt);

public record CreateWarehouseRequest(
    string Name,
    string Code,
    string AddressLine1,
    string City,
    string District,
    string PostalCode,
    string? AddressLine2 = null,
    string Country = "BD",
    string? Phone = null,
    string? Email = null,
    bool IsDefault = false);

public record UpdateWarehouseRequest(
    string Name,
    string AddressLine1,
    string City,
    string District,
    string PostalCode,
    string? AddressLine2 = null,
    string Country = "BD",
    string? Phone = null,
    string? Email = null);

public record DistrictResponse(string Code, string Name);

public record TransferWarehouseToStoreRequest(
    Guid WarehouseId,
    Guid StoreId,
    Guid ProductId,
    decimal Quantity,
    string? Notes = null);
