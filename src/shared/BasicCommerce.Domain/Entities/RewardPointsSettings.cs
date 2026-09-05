using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Entities;

/// <summary>
/// Tenant-level reward points programme configuration. One record per tenant.
/// </summary>
public class RewardPointsSettings : TenantEntity
{
    /// <summary>1 reward point = ExchangeRate currency units.</summary>
    public decimal ExchangeRate { get; private set; } = 1m;

    /// <summary>Minimum accumulated points before a customer may redeem. 0 = no minimum.</summary>
    public int MinimumPointsToUse { get; private set; } = 0;

    /// <summary>Max points redeemable per order. 0 = unlimited.</summary>
    public int MaximumPointsPerOrder { get; private set; } = 0;

    /// <summary>Max fraction (0–1) of order total payable by points. 0 = unlimited.</summary>
    public decimal MaximumRedeemedRate { get; private set; } = 0m;

    /// <summary>Spend PurchaseSpendPerPoint currency units to earn PointsEarnedPerSpend points.</summary>
    public decimal PurchaseSpendPerPoint { get; private set; } = 10m;
    public int PointsEarnedPerSpend { get; private set; } = 1;

    /// <summary>Days until purchase-earned points expire. 0 = never.</summary>
    public int PurchasePointsValidityDays { get; private set; } = 45;

    /// <summary>Minimum order subtotal (excl. shipping) required to earn purchase points. 0 = no minimum.</summary>
    public decimal MinimumOrderTotalForPoints { get; private set; } = 0m;

    /// <summary>Points awarded on new customer registration. 0 = none.</summary>
    public int PointsForRegistration { get; private set; } = 0;

    /// <summary>Days until registration-earned points expire. 0 = never.</summary>
    public int RegistrationPointsValidityDays { get; private set; } = 30;

    /// <summary>If false, earned points go to a pending state until manually activated.</summary>
    public bool ActivatePointsImmediately { get; private set; } = true;

    /// <summary>Show estimated points at checkout before order is placed.</summary>
    public bool DisplayHowMuchWillBeEarned { get; private set; } = true;

    /// <summary>
    /// When true, all stores share one reward balance per customer.
    /// When false, each store maintains its own customer balance.
    /// </summary>
    public bool PointsAccumulatedForAllStores { get; private set; } = true;

    private RewardPointsSettings() { }

    public static RewardPointsSettings CreateDefault(Guid tenantId) =>
        new() { TenantId = tenantId };

    public void Update(
        decimal exchangeRate,
        int minimumPointsToUse,
        int maximumPointsPerOrder,
        decimal maximumRedeemedRate,
        decimal purchaseSpendPerPoint,
        int pointsEarnedPerSpend,
        int purchasePointsValidityDays,
        decimal minimumOrderTotalForPoints,
        int pointsForRegistration,
        int registrationPointsValidityDays,
        bool activatePointsImmediately,
        bool displayHowMuchWillBeEarned,
        bool pointsAccumulatedForAllStores)
    {
        ExchangeRate = exchangeRate > 0 ? exchangeRate : 1m;
        MinimumPointsToUse = Math.Max(0, minimumPointsToUse);
        MaximumPointsPerOrder = Math.Max(0, maximumPointsPerOrder);
        MaximumRedeemedRate = Math.Clamp(maximumRedeemedRate, 0m, 1m);
        PurchaseSpendPerPoint = purchaseSpendPerPoint > 0 ? purchaseSpendPerPoint : 1m;
        PointsEarnedPerSpend = Math.Max(1, pointsEarnedPerSpend);
        PurchasePointsValidityDays = Math.Max(0, purchasePointsValidityDays);
        MinimumOrderTotalForPoints = Math.Max(0, minimumOrderTotalForPoints);
        PointsForRegistration = Math.Max(0, pointsForRegistration);
        RegistrationPointsValidityDays = Math.Max(0, registrationPointsValidityDays);
        ActivatePointsImmediately = activatePointsImmediately;
        DisplayHowMuchWillBeEarned = displayHowMuchWillBeEarned;
        PointsAccumulatedForAllStores = pointsAccumulatedForAllStores;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Calculate points earned for a given order total.</summary>
    public int CalculatePurchasePoints(decimal orderTotal)
    {
        if (Status != EntityStatus.Active) return 0;
        if (MinimumOrderTotalForPoints > 0 && orderTotal < MinimumOrderTotalForPoints) return 0;
        if (PurchaseSpendPerPoint <= 0) return 0;
        return (int)Math.Floor(orderTotal / PurchaseSpendPerPoint) * PointsEarnedPerSpend;
    }

    /// <summary>
    /// Given the customer's available points and order total, returns how many points
    /// are actually redeemable respecting all configured limits.
    /// </summary>
    /// <param name="alreadyRedeemedThisOrder">
    /// Points already redeemed against this same order in an earlier split-payment
    /// call. The per-order caps (<see cref="MaximumPointsPerOrder"/>,
    /// <see cref="MaximumRedeemedRate"/>, and the order-total ceiling) apply to the
    /// order as a whole, so this amount is subtracted from each of them — otherwise a
    /// customer could exceed a per-order cap by splitting the points payment into
    /// several smaller calls.
    /// </param>
    public int CalculateMaxRedeemablePoints(int availablePoints, decimal orderTotal,
        int alreadyRedeemedThisOrder = 0)
    {
        if (availablePoints <= 0) return 0;
        if (MinimumPointsToUse > 0 && availablePoints < MinimumPointsToUse) return 0;

        var redeemable = availablePoints;

        if (MaximumPointsPerOrder > 0)
            redeemable = Math.Min(redeemable, Math.Max(0, MaximumPointsPerOrder - alreadyRedeemedThisOrder));

        if (MaximumRedeemedRate > 0 && ExchangeRate > 0)
        {
            var maxCurrency = orderTotal * MaximumRedeemedRate;
            var maxByRate = (int)Math.Floor(maxCurrency / ExchangeRate);
            redeemable = Math.Min(redeemable, Math.Max(0, maxByRate - alreadyRedeemedThisOrder));
        }

        // Cannot redeem more than the order total in currency terms
        if (ExchangeRate > 0)
        {
            var pointsWorthOrderTotal = (int)Math.Ceiling(orderTotal / ExchangeRate);
            redeemable = Math.Min(redeemable, Math.Max(0, pointsWorthOrderTotal - alreadyRedeemedThisOrder));
        }

        return Math.Max(0, redeemable);
    }

    public void Deactivate() { Status = EntityStatus.Inactive; UpdatedAt = DateTime.UtcNow; }
    public void Activate() { Status = EntityStatus.Active; UpdatedAt = DateTime.UtcNow; }
}
