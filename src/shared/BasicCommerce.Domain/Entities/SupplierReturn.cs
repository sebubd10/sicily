using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;

namespace BasicCommerce.Domain.Entities;

public class SupplierReturn : TenantEntity
{
    public string ReturnNumber { get; private set; } = default!;
    public Guid SupplierId { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid? PurchaseOrderId { get; private set; }
    public SupplierReturnStatus ReturnStatus { get; private set; } = SupplierReturnStatus.Draft;
    public string? Notes { get; private set; }
    public decimal? ExpectedCreditAmount { get; private set; }
    public decimal? ActualCreditAmount { get; private set; }
    public string? CreditNoteReference { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? CreditReceivedAt { get; private set; }

    public Supplier? Supplier { get; private set; }
    public Store? Store { get; private set; }
    public PurchaseOrder? PurchaseOrder { get; private set; }

    private readonly List<SupplierReturnItem> _items = [];
    public IReadOnlyCollection<SupplierReturnItem> Items => _items.AsReadOnly();

    public decimal TotalReturnValue => _items.Sum(i => i.TotalCost);

    private SupplierReturn() { }

    public static SupplierReturn Create(Guid tenantId, Guid supplierId, Guid storeId,
        Guid? purchaseOrderId = null, string? notes = null)
    {
        return new SupplierReturn
        {
            TenantId = tenantId,
            SupplierId = supplierId,
            StoreId = storeId,
            PurchaseOrderId = purchaseOrderId,
            Notes = notes,
            ReturnNumber = GenerateNumber()
        };
    }

    public SupplierReturnItem AddItem(Guid productId, decimal quantity,
        decimal unitCost, SupplierReturnReason reason, string? notes = null)
    {
        if (ReturnStatus != SupplierReturnStatus.Draft)
            throw new DomainException("Items can only be added to a Draft supplier return.");

        var existing = _items.FirstOrDefault(i => i.ProductId == productId && i.Reason == reason);
        if (existing is not null)
            throw new DomainException(
                $"Product {productId} with reason {reason} is already in this return. Remove and re-add to change quantity.");

        var item = SupplierReturnItem.Create(Id, productId, quantity, unitCost, reason, notes);
        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
        return item;
    }

    public void RemoveItem(Guid productId)
    {
        if (ReturnStatus != SupplierReturnStatus.Draft)
            throw new DomainException("Items can only be removed from a Draft supplier return.");

        var item = _items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new NotFoundException("SupplierReturnItem", productId);
        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Submit()
    {
        if (ReturnStatus != SupplierReturnStatus.Draft)
            throw new DomainException("Only Draft returns can be submitted.");
        if (_items.Count == 0)
            throw new DomainException("Cannot submit a supplier return with no items.");
        ReturnStatus = SupplierReturnStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkShipped()
    {
        if (ReturnStatus != SupplierReturnStatus.Submitted)
            throw new DomainException("Only Submitted returns can be marked as shipped.");
        ReturnStatus = SupplierReturnStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReceiveCredit(decimal creditAmount, string? creditNoteReference)
    {
        if (ReturnStatus != SupplierReturnStatus.Shipped)
            throw new DomainException("Only Shipped returns can have credit received.");
        ReturnStatus = SupplierReturnStatus.CreditReceived;
        ActualCreditAmount = creditAmount;
        CreditNoteReference = creditNoteReference;
        CreditReceivedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetExpectedCredit(decimal amount)
    {
        if (ReturnStatus == SupplierReturnStatus.CreditReceived)
            throw new DomainException("Cannot change expected credit after credit is received.");
        ExpectedCreditAmount = amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (ReturnStatus is SupplierReturnStatus.Shipped or SupplierReturnStatus.CreditReceived)
            throw new DomainException("Shipped or credited returns cannot be cancelled.");
        ReturnStatus = SupplierReturnStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateNumber() =>
        $"SR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
}

public class SupplierReturnItem : BaseEntity
{
    public Guid SupplierReturnId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public SupplierReturnReason Reason { get; private set; }
    public string? Notes { get; private set; }

    public Product? Product { get; private set; }

    public decimal TotalCost => Quantity * UnitCost;

    private SupplierReturnItem() { }

    internal static SupplierReturnItem Create(Guid supplierReturnId, Guid productId,
        decimal quantity, decimal unitCost, SupplierReturnReason reason, string? notes)
    {
        if (quantity <= 0) throw new DomainException("Quantity must be greater than zero.");
        if (unitCost < 0) throw new DomainException("Unit cost cannot be negative.");

        return new SupplierReturnItem
        {
            SupplierReturnId = supplierReturnId,
            ProductId = productId,
            Quantity = quantity,
            UnitCost = unitCost,
            Reason = reason,
            Notes = notes
        };
    }
}
