using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Entities;

public class StockMovement : TenantEntity
{
    public Guid StoreId { get; private set; }
    public Guid ProductId { get; private set; }
    public StockMovementType Type { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal QuantityBefore { get; private set; }
    public decimal QuantityAfter { get; private set; }
    public Guid? RelatedStoreId { get; private set; }
    public Guid? ReferenceTransactionId { get; private set; }
    public string? Reference { get; private set; }
    public string? Notes { get; private set; }
    public Guid RecordedByUserId { get; private set; }

    public Product? Product { get; private set; }

    private StockMovement() { }

    public static StockMovement Create(Guid tenantId, Guid storeId, Guid productId,
        StockMovementType type, decimal quantity, decimal quantityBefore,
        Guid recordedByUserId, string? reference = null, string? notes = null,
        Guid? relatedStoreId = null)
    {
        return new StockMovement
        {
            TenantId = tenantId,
            StoreId = storeId,
            ProductId = productId,
            Type = type,
            Quantity = quantity,
            QuantityBefore = quantityBefore,
            QuantityAfter = quantityBefore + quantity,
            RecordedByUserId = recordedByUserId,
            Reference = reference,
            Notes = notes,
            RelatedStoreId = relatedStoreId
        };
    }
}
