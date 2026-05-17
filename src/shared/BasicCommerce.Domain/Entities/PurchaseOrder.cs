using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;

namespace BasicCommerce.Domain.Entities;

public class PurchaseOrder : TenantEntity
{
    public string OrderNumber { get; private set; } = default!;
    public Guid SupplierId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public PurchaseOrderStatus PurchaseOrderStatus { get; private set; } = PurchaseOrderStatus.Draft;
    public DateTime OrderDate { get; private set; }
    public DateTime? ExpectedDate { get; private set; }
    public DateTime? ReceivedDate { get; private set; }
    public string? Notes { get; private set; }
    public string Currency { get; private set; } = "BDT";

    public Supplier? Supplier { get; private set; }
    public Warehouse? Warehouse { get; private set; }

    private readonly List<PurchaseOrderItem> _items = [];
    public IReadOnlyCollection<PurchaseOrderItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(i => i.TotalCost);

    private PurchaseOrder() { }

    public static PurchaseOrder Create(Guid tenantId, string orderNumber, Guid supplierId,
        Guid warehouseId, DateTime orderDate, DateTime? expectedDate = null,
        string? notes = null, string currency = "BDT")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);
        return new PurchaseOrder
        {
            TenantId = tenantId,
            OrderNumber = orderNumber,
            SupplierId = supplierId,
            WarehouseId = warehouseId,
            OrderDate = orderDate,
            ExpectedDate = expectedDate,
            Notes = notes,
            Currency = currency
        };
    }

    public void AddItem(Guid productId, decimal orderedQuantity, decimal unitCost)
    {
        if (PurchaseOrderStatus != PurchaseOrderStatus.Draft)
            throw new DomainException("Items can only be added to a Draft purchase order.");
        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is not null)
            existing.UpdateQuantity(orderedQuantity, unitCost);
        else
            _items.Add(PurchaseOrderItem.Create(Id, productId, orderedQuantity, unitCost));
    }

    public void RemoveItem(Guid productId)
    {
        if (PurchaseOrderStatus != PurchaseOrderStatus.Draft)
            throw new DomainException("Items can only be removed from a Draft purchase order.");
        var item = _items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new NotFoundException("PurchaseOrderItem", productId);
        _items.Remove(item);
    }

    public void Submit()
    {
        if (PurchaseOrderStatus != PurchaseOrderStatus.Draft)
            throw new DomainException("Only Draft orders can be submitted.");
        if (_items.Count == 0)
            throw new DomainException("Cannot submit a purchase order with no items.");
        PurchaseOrderStatus = PurchaseOrderStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkPartiallyReceived()
    {
        if (PurchaseOrderStatus is not (PurchaseOrderStatus.Submitted or PurchaseOrderStatus.PartiallyReceived))
            throw new DomainException("Only Submitted or PartiallyReceived orders can be partially received.");
        PurchaseOrderStatus = PurchaseOrderStatus.PartiallyReceived;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkReceived()
    {
        PurchaseOrderStatus = PurchaseOrderStatus.Received;
        ReceivedDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (PurchaseOrderStatus is PurchaseOrderStatus.Received)
            throw new DomainException("Received orders cannot be cancelled.");
        PurchaseOrderStatus = PurchaseOrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}
