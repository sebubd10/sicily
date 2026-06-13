using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Application.Features.Products;

internal static class ProductMapper
{
    internal static ProductResponse ToResponse(Product p, VatRate? vatRate,
        string? categoryName = null, string? manufacturerName = null, string? categoryStatus = null) =>
        new(p.Id,
            p.Sku,
            p.Barcode,
            p.Plu,
            p.Name,
            p.NameBn,
            p.Description,
            p.CategoryId,
            categoryName ?? p.Category?.Name ?? string.Empty,
            categoryStatus ?? p.Category?.Status.ToString() ?? "Active",
            p.Price.Amount,
            p.Price.Currency,
            p.CostPrice?.Amount,
            p.VatRateId,
            vatRate?.Name ?? string.Empty,
            vatRate?.Rate ?? 0,
            p.UnitType.ToString(),
            p.UnitLabel,
            p.IsWeightBased,
            p.IsPerishable,
            p.IsAgeRestricted,
            p.AgeRestrictionYears,
            p.IsEbtEligible,
            p.TrackInventory,
            p.ReorderLevel,
            p.Status.ToString(),
            p.ImageUrl,
            p.ManufacturerId,
            manufacturerName ?? p.Manufacturer?.Name,
            p.Tags.Select(t => new ProductTagResponse(t.Id, t.Name)).ToList(),
            p.CreatedAt,
            p.UpdatedAt);
}
