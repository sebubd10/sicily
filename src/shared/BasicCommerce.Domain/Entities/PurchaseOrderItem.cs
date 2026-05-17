namespace BasicCommerce.Domain.Entities;

public class PurchaseOrderItem : BaseEntity
{
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal OrderedQuantity { get; private set; }
    public decimal ReceivedQuantity { get; private set; }
    public decimal UnitCost { get; private set; }

    public Product? Product { get; private set; }

    public decimal TotalCost => OrderedQuantity * UnitCost;
    public decimal RemainingQuantity => OrderedQuantity - ReceivedQuantity;
    public bool IsFullyReceived => ReceivedQuantity >= OrderedQuantity;

    private PurchaseOrderItem() { }

    public static PurchaseOrderItem Create(Guid purchaseOrderId, Guid productId,
        decimal orderedQuantity, decimal unitCost) =>
        new()
        {
            PurchaseOrderId = purchaseOrderId,
            ProductId = productId,
            OrderedQuantity = orderedQuantity,
            UnitCost = unitCost
        };

    public void UpdateQuantity(decimal orderedQuantity, decimal unitCost)
    {
        OrderedQuantity = orderedQuantity;
        UnitCost = unitCost;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Receive(decimal quantity)
    {
        if (quantity > RemainingQuantity)
            throw new Domain.Exceptions.DomainException(
                $"Cannot receive {quantity}. Remaining: {RemainingQuantity}.");
        ReceivedQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
