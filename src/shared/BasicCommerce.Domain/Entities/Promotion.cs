using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;

namespace BasicCommerce.Domain.Entities;

public class Promotion : TenantEntity
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public PromotionType Type { get; private set; }
    public PromotionStatus PromotionStatus { get; private set; } = PromotionStatus.Draft;

    // Applicability
    public Guid? ProductId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Guid? StoreId { get; private set; }

    // Discount values
    public decimal? DiscountPercentage { get; private set; }
    public decimal? DiscountAmount { get; private set; }

    // Buy X Get Y
    public int? BuyQuantity { get; private set; }
    public int? GetQuantity { get; private set; }

    // Cart discount
    public decimal? MinimumCartValue { get; private set; }

    // Coupon
    public string? CouponCode { get; private set; }
    public bool RequiresCoupon { get; private set; }

    // Validity
    public DateTime? StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }

    // Usage limits
    public int? MaxUses { get; private set; }
    public int UsedCount { get; private set; }

    private Promotion() { }

    public static Promotion Create(Guid tenantId, string name, string? description,
        PromotionType type, Guid? productId, Guid? categoryId, Guid? storeId,
        decimal? discountPercentage, decimal? discountAmount,
        int? buyQty, int? getQty,
        decimal? minimumCartValue, string? couponCode, bool requiresCoupon,
        DateTime? startsAt, DateTime? endsAt, int? maxUses)
    {
        ValidateByType(type, productId, categoryId, discountPercentage, discountAmount,
            buyQty, getQty, minimumCartValue);

        return new Promotion
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Description = description,
            Type = type,
            ProductId = productId,
            CategoryId = categoryId,
            StoreId = storeId,
            DiscountPercentage = discountPercentage,
            DiscountAmount = discountAmount,
            BuyQuantity = buyQty,
            GetQuantity = getQty,
            MinimumCartValue = minimumCartValue,
            CouponCode = couponCode?.Trim().ToUpperInvariant(),
            RequiresCoupon = requiresCoupon,
            StartsAt = startsAt,
            EndsAt = endsAt,
            MaxUses = maxUses
        };
    }

    public void Activate()
    {
        if (PromotionStatus == PromotionStatus.Cancelled)
            throw new DomainException("Cancelled promotions cannot be activated.");
        PromotionStatus = PromotionStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Pause()
    {
        if (PromotionStatus != PromotionStatus.Active)
            throw new DomainException("Only active promotions can be paused.");
        PromotionStatus = PromotionStatus.Paused;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (PromotionStatus == PromotionStatus.Cancelled)
            throw new DomainException("Promotion is already cancelled.");
        PromotionStatus = PromotionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordUse()
    {
        UsedCount++;
        if (MaxUses.HasValue && UsedCount >= MaxUses.Value)
        {
            PromotionStatus = PromotionStatus.Expired;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsApplicableNow(DateTime? asOf = null)
    {
        var now = asOf ?? DateTime.UtcNow;
        if (PromotionStatus != PromotionStatus.Active) return false;
        if (StartsAt.HasValue && now < StartsAt.Value) return false;
        if (EndsAt.HasValue && now > EndsAt.Value) return false;
        if (MaxUses.HasValue && UsedCount >= MaxUses.Value) return false;
        return true;
    }

    public decimal CalculateDiscount(decimal lineTotal, decimal quantity)
    {
        return Type switch
        {
            PromotionType.PercentageOff when DiscountPercentage.HasValue =>
                Math.Round(lineTotal * DiscountPercentage.Value / 100m, 4),
            PromotionType.FixedAmountOff when DiscountAmount.HasValue =>
                Math.Min(DiscountAmount.Value, lineTotal),
            PromotionType.BuyXGetYFree when BuyQuantity.HasValue && GetQuantity.HasValue =>
                CalculateBxGy(lineTotal, quantity),
            PromotionType.CategoryPercentageOff when DiscountPercentage.HasValue =>
                Math.Round(lineTotal * DiscountPercentage.Value / 100m, 4),
            _ => 0m
        };
    }

    private decimal CalculateBxGy(decimal lineTotal, decimal quantity)
    {
        if (BuyQuantity is null || GetQuantity is null || quantity <= 0) return 0m;
        var sets = Math.Floor(quantity / (BuyQuantity.Value + GetQuantity.Value));
        var freeQty = sets * GetQuantity.Value;
        var unitPrice = lineTotal / quantity;
        return Math.Round(freeQty * unitPrice, 4);
    }

    private static void ValidateByType(PromotionType type,
        Guid? productId, Guid? categoryId,
        decimal? discountPct, decimal? discountAmt,
        int? buyQty, int? getQty, decimal? minCartValue)
    {
        switch (type)
        {
            case PromotionType.PercentageOff:
                if (!discountPct.HasValue || discountPct <= 0 || discountPct > 100)
                    throw new DomainException("PercentageOff requires DiscountPercentage 1-100.");
                if (productId is null)
                    throw new DomainException("PercentageOff requires a target product.");
                break;

            case PromotionType.FixedAmountOff:
                if (!discountAmt.HasValue || discountAmt <= 0)
                    throw new DomainException("FixedAmountOff requires a positive DiscountAmount.");
                if (productId is null)
                    throw new DomainException("FixedAmountOff requires a target product.");
                break;

            case PromotionType.BuyXGetYFree:
                if (!buyQty.HasValue || buyQty <= 0)
                    throw new DomainException("BuyXGetYFree requires BuyQuantity > 0.");
                if (!getQty.HasValue || getQty <= 0)
                    throw new DomainException("BuyXGetYFree requires GetQuantity > 0.");
                if (productId is null)
                    throw new DomainException("BuyXGetYFree requires a target product.");
                break;

            case PromotionType.CategoryPercentageOff:
                if (!discountPct.HasValue || discountPct <= 0 || discountPct > 100)
                    throw new DomainException("CategoryPercentageOff requires DiscountPercentage 1-100.");
                if (categoryId is null)
                    throw new DomainException("CategoryPercentageOff requires a target category.");
                break;

            case PromotionType.CartDiscount:
                if (!discountPct.HasValue && !discountAmt.HasValue)
                    throw new DomainException("CartDiscount requires DiscountPercentage or DiscountAmount.");
                break;
        }
    }
}
