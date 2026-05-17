using BasicCommerce.Domain.Exceptions;

namespace BasicCommerce.Domain.Entities;

public class SupplierProduct : TenantEntity
{
    public Guid SupplierId { get; private set; }
    public Guid ProductId { get; private set; }
    public string? SupplierSku { get; private set; }
    public decimal UnitCost { get; private set; }
    public string CurrencyCode { get; private set; } = "BDT";
    public int? MinOrderQuantity { get; private set; }
    public int? LeadTimeDays { get; private set; }
    public string? Notes { get; private set; }
    public DateTime PriceLastConfirmedAt { get; private set; }

    public Supplier? Supplier { get; private set; }
    public Product? Product { get; private set; }

    private SupplierProduct() { }

    public static SupplierProduct Create(Guid tenantId, Guid supplierId, Guid productId,
        decimal unitCost, string currencyCode = "BDT", string? supplierSku = null,
        int? minOrderQuantity = null, int? leadTimeDays = null, string? notes = null)
    {
        if (unitCost < 0) throw new DomainException("Unit cost cannot be negative.");
        return new SupplierProduct
        {
            TenantId = tenantId,
            SupplierId = supplierId,
            ProductId = productId,
            UnitCost = unitCost,
            CurrencyCode = currencyCode,
            SupplierSku = supplierSku,
            MinOrderQuantity = minOrderQuantity,
            LeadTimeDays = leadTimeDays,
            Notes = notes,
            PriceLastConfirmedAt = DateTime.UtcNow
        };
    }

    public void Update(decimal unitCost, string? supplierSku,
        int? minOrderQuantity, int? leadTimeDays, string? notes, string? currencyCode)
    {
        if (unitCost < 0) throw new DomainException("Unit cost cannot be negative.");
        UnitCost = unitCost;
        SupplierSku = supplierSku;
        MinOrderQuantity = minOrderQuantity;
        LeadTimeDays = leadTimeDays;
        Notes = notes;
        if (!string.IsNullOrWhiteSpace(currencyCode)) CurrencyCode = currencyCode;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmPrice(decimal confirmedCost)
    {
        if (confirmedCost < 0) throw new DomainException("Unit cost cannot be negative.");
        UnitCost = confirmedCost;
        PriceLastConfirmedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
