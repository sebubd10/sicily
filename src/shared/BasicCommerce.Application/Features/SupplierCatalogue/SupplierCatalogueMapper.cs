using BasicCommerce.Contracts.Suppliers;
using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Application.Features.SupplierCatalogue;

public static class SupplierCatalogueMapper
{
    public static SupplierProductResponse ToResponse(
        SupplierProduct sp, string supplierName, string productName, string productSku)
        => new(sp.Id, sp.SupplierId, supplierName,
            sp.ProductId, productName, productSku,
            sp.SupplierSku, sp.UnitCost, sp.CurrencyCode,
            sp.MinOrderQuantity, sp.LeadTimeDays, sp.Notes,
            sp.PriceLastConfirmedAt);
}
