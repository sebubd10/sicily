using BasicCommerce.Domain.Exceptions;

namespace BasicCommerce.Domain.Entities;

public class StockLevel : TenantEntity
{
    public Guid StoreId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal ReservedQuantity { get; private set; }
    public decimal LowStockThreshold { get; private set; } = 10;
    public DateTime? LastCountedAt { get; private set; }

    public decimal AvailableQuantity => Quantity - ReservedQuantity;
    public bool IsLowStock => AvailableQuantity <= LowStockThreshold;

    private StockLevel() { }

    public static StockLevel Create(Guid tenantId, Guid storeId, Guid productId,
        decimal initialQuantity = 0, decimal lowStockThreshold = 10)
    {
        return new StockLevel
        {
            TenantId = tenantId,
            StoreId = storeId,
            ProductId = productId,
            Quantity = initialQuantity,
            LowStockThreshold = lowStockThreshold
        };
    }

    public void Decrement(decimal quantity)
    {
        if (quantity > AvailableQuantity)
            throw new DomainException($"Insufficient stock. Available: {AvailableQuantity}.");
        Quantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Increment(decimal quantity)
    {
        Quantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AdjustCount(decimal newQuantity, Guid countedBy)
    {
        Quantity = newQuantity;
        LastCountedAt = DateTime.UtcNow;
        SetUpdated(countedBy);
    }

    public void SetLowStockThreshold(decimal threshold) => LowStockThreshold = threshold;
}
