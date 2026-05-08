using BasicCommerce.Contracts.Promotions;
using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Application.Features.Promotions;

public static class PromotionMapper
{
    public static PromotionResponse ToResponse(Promotion p)
        => new(p.Id, p.Name, p.Description, p.Type.ToString(), p.PromotionStatus.ToString(),
            p.ProductId, p.CategoryId, p.StoreId,
            p.DiscountPercentage, p.DiscountAmount,
            p.BuyQuantity, p.GetQuantity, p.MinimumCartValue,
            p.CouponCode, p.RequiresCoupon,
            p.StartsAt, p.EndsAt, p.MaxUses, p.UsedCount, p.CreatedAt);

    public static PromotionSummaryResponse ToSummary(Promotion p)
        => new(p.Id, p.Name, p.Type.ToString(), p.PromotionStatus.ToString(),
            p.DiscountPercentage, p.DiscountAmount, p.StartsAt, p.EndsAt);
}
