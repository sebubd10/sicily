using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;

namespace BasicCommerce.Application.Features.Transactions;

/// <summary>
/// Pricing rules shared by the real sale (AddLineItemCommand) and the cart preview,
/// so the amount shown to the cashier is exactly what gets charged.
/// </summary>
internal static class CartPricing
{
    public static async Task<decimal> GetVatRateAsync(
        IUnitOfWork uow, Guid tenantId, Product product, CancellationToken ct)
    {
        var vat = await uow.VatRates.GetByIdForTenantAsync(tenantId, product.VatRateId, ct);
        return vat?.Rate ?? 0m;
    }

    /// <summary>Best non-coupon promotion for the line, or null if none applies.</summary>
    public static (Promotion? Promotion, decimal Discount) FindBestPromotion(
        IEnumerable<Promotion> activePromotions, Product product, LineItem line)
    {
        Promotion? best = null;
        decimal bestDiscount = 0;

        foreach (var promo in activePromotions.Where(p => !p.RequiresCoupon))
        {
            var applies = promo.Type switch
            {
                PromotionType.PercentageOff => promo.ProductId == product.Id,
                PromotionType.FixedAmountOff => promo.ProductId == product.Id,
                PromotionType.BuyXGetYFree => promo.ProductId == product.Id,
                PromotionType.CategoryPercentageOff => promo.CategoryId == product.CategoryId,
                _ => false
            };
            if (!applies) continue;

            var discount = promo.CalculateDiscount(line.LineTotal, line.Quantity);
            if (discount > bestDiscount)
            {
                bestDiscount = discount;
                best = promo;
            }
        }

        return (best, bestDiscount);
    }
}
