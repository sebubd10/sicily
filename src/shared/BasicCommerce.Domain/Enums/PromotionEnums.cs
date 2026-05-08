namespace BasicCommerce.Domain.Enums;

public enum PromotionType
{
    PercentageOff = 1,
    FixedAmountOff = 2,
    BuyXGetYFree = 3,
    CategoryPercentageOff = 4,
    CartDiscount = 5
}

public enum PromotionStatus
{
    Draft = 1,
    Active = 2,
    Paused = 3,
    Expired = 4,
    Cancelled = 5
}
