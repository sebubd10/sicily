namespace BasicCommerce.Domain.Entities;

public class WarehouseStockLevel : TenantEntity
{
    public Guid WarehouseId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal ReservedQuantity { get; private set; }
    public decimal LowStockThreshold { get; private set; } = 10;

    public Warehouse? Warehouse { get; private set; }
    public Product? Product { get; private set; }

    public decimal AvailableQuantity => Quantity - ReservedQuantity;
    public bool IsLowStock => AvailableQuantity <= LowStockThreshold;

    private WarehouseStockLevel() { }

    public static WarehouseStockLevel Create(Guid tenantId, Guid warehouseId, Guid productId,
        decimal initialQuantity = 0, decimal lowStockThreshold = 10) =>
        new()
        {
            TenantId = tenantId,
            WarehouseId = warehouseId,
            ProductId = productId,
            Quantity = initialQuantity,
            LowStockThreshold = lowStockThreshold
        };

    public void Increment(decimal quantity)
    {
        Quantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Decrement(decimal quantity)
    {
        if (quantity > AvailableQuantity)
            throw new Domain.Exceptions.DomainException($"Insufficient warehouse stock. Available: {AvailableQuantity}.");
        Quantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reserve(decimal quantity)
    {
        if (quantity > AvailableQuantity)
            throw new Domain.Exceptions.DomainException($"Insufficient available quantity to reserve: {AvailableQuantity}.");
        ReservedQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseReservation(decimal quantity)
    {
        ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetLowStockThreshold(decimal threshold) => LowStockThreshold = threshold;
}
