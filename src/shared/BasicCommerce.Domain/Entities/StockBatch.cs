using BasicCommerce.Domain.Exceptions;

namespace BasicCommerce.Domain.Entities;

public class StockBatch : TenantEntity
{
    public Guid StoreId { get; private set; }
    public Guid ProductId { get; private set; }
    public string? LotNumber { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public decimal ReceivedQuantity { get; private set; }
    public decimal RemainingQuantity { get; private set; }
    public decimal? UnitCost { get; private set; }
    public Guid? PurchaseOrderId { get; private set; }
    public bool IsExpired { get; private set; }

    public Product? Product { get; private set; }
    public Store? Store { get; private set; }

    private StockBatch() { }

    public static StockBatch Create(Guid tenantId, Guid storeId, Guid productId,
        decimal quantity, DateTime? expiryDate = null, string? lotNumber = null,
        decimal? unitCost = null, Guid? purchaseOrderId = null)
    {
        if (quantity <= 0)
            throw new DomainException("Batch quantity must be greater than zero.");

        return new StockBatch
        {
            TenantId = tenantId,
            StoreId = storeId,
            ProductId = productId,
            ReceivedQuantity = quantity,
            RemainingQuantity = quantity,
            ExpiryDate = expiryDate,
            LotNumber = lotNumber,
            UnitCost = unitCost,
            PurchaseOrderId = purchaseOrderId
        };
    }

    public decimal Consume(decimal quantity)
    {
        if (IsExpired)
            throw new DomainException($"Cannot consume from expired batch {LotNumber ?? Id.ToString()}.");

        var actual = Math.Min(quantity, RemainingQuantity);
        RemainingQuantity -= actual;
        UpdatedAt = DateTime.UtcNow;
        return actual;
    }

    public void MarkExpired()
    {
        IsExpired = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
