namespace BasicCommerce.Domain.Entities;

public class LineItem : BaseEntity
{
    public Guid TransactionId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public string ProductSku { get; private set; } = default!;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal LineTotal { get; private set; }
    public bool IsPriceOverridden { get; private set; }
    public Guid? PriceOverrideApprovedBy { get; private set; }
    public bool IsVoided { get; private set; }
    public Guid? VoidedBy { get; private set; }
    public DateTime? VoidedAt { get; private set; }

    private LineItem() { }

    internal static LineItem Create(Guid transactionId, Product product, decimal quantity,
        decimal unitPrice, Guid? overrideApprovedBy = null)
    {
        var lineTotal = unitPrice * quantity;

        return new LineItem
        {
            TransactionId = transactionId,
            ProductId = product.Id,
            ProductName = product.Name,
            ProductSku = product.Sku,
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = lineTotal,
            IsPriceOverridden = overrideApprovedBy.HasValue,
            PriceOverrideApprovedBy = overrideApprovedBy
        };
    }

    internal void ApplyTax(decimal taxRate)
    {
        TaxRate = taxRate;
        TaxAmount = LineTotal * (taxRate / 100);
    }

    internal void ApplyDiscount(decimal discountAmount)
    {
        DiscountAmount = discountAmount;
    }

    internal void Void(Guid voidedBy)
    {
        IsVoided = true;
        VoidedBy = voidedBy;
        VoidedAt = DateTime.UtcNow;
    }
}
