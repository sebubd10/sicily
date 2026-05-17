using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Entities;

public class WarehouseMovement : TenantEntity
{
    public Guid WarehouseId { get; private set; }
    public Guid ProductId { get; private set; }
    public WarehouseMovementType MovementType { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal QuantityBefore { get; private set; }
    public decimal QuantityAfter { get; private set; }
    public Guid? RelatedStoreId { get; private set; }
    public Guid? PurchaseOrderId { get; private set; }
    public string? Reference { get; private set; }
    public string? Notes { get; private set; }
    public Guid RecordedByUserId { get; private set; }

    public Warehouse? Warehouse { get; private set; }
    public Product? Product { get; private set; }

    private WarehouseMovement() { }

    public static WarehouseMovement Create(Guid tenantId, Guid warehouseId, Guid productId,
        WarehouseMovementType movementType, decimal quantity, decimal quantityBefore,
        Guid recordedByUserId, string? reference = null, string? notes = null,
        Guid? relatedStoreId = null, Guid? purchaseOrderId = null) =>
        new()
        {
            TenantId = tenantId,
            WarehouseId = warehouseId,
            ProductId = productId,
            MovementType = movementType,
            Quantity = quantity,
            QuantityBefore = quantityBefore,
            QuantityAfter = quantityBefore + quantity,
            RecordedByUserId = recordedByUserId,
            Reference = reference,
            Notes = notes,
            RelatedStoreId = relatedStoreId,
            PurchaseOrderId = purchaseOrderId
        };
}
