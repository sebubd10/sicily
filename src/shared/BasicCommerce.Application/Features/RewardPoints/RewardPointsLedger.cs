using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;

namespace BasicCommerce.Application.Features.RewardPoints;

/// <summary>
/// Shared reward-points operations used by sales, returns, voids and checkout, so every
/// path resolves the right account and applies expiry the same way.
/// </summary>
internal static class RewardPointsLedger
{
    /// <summary>Account scope: null when one balance is shared across all stores.</summary>
    public static Guid? AccountStoreId(RewardPointsSettings settings, Guid storeId) =>
        settings.PointsAccumulatedForAllStores ? null : storeId;

    public static async Task<RewardPointsAccount?> GetAccountAsync(IUnitOfWork uow,
        RewardPointsSettings settings, Guid tenantId, Guid customerId, Guid storeId,
        CancellationToken ct) =>
        await uow.RewardPointsAccounts.GetByCustomerAsync(
            tenantId, customerId, AccountStoreId(settings, storeId), ct);

    /// <summary>
    /// Expires lapsed points on the account and keeps the customer's summary in sync.
    /// The caller decides whether to save.
    /// </summary>
    public static async Task ApplyExpiryAsync(IUnitOfWork uow, RewardPointsAccount account,
        CancellationToken ct)
    {
        var expiredEarned = await uow.RewardPointsAccounts
            .GetExpiredEarnedTotalAsync(account.Id, DateTime.UtcNow, ct);
        var expired = account.ApplyExpiry(expiredEarned);
        if (expired > 0)
            await AdjustCustomerSummaryAsync(uow, account.CustomerId, -expired, ct);
    }

    /// <summary>
    /// Customer.LoyaltyPoints is a denormalised total shown in customer lists; keep it in
    /// step with the ledger without ever going negative.
    /// </summary>
    public static async Task AdjustCustomerSummaryAsync(IUnitOfWork uow, Guid customerId,
        int delta, CancellationToken ct)
    {
        if (delta == 0) return;
        var customer = await uow.Customers.GetByIdAsync(customerId, ct);
        if (customer is null) return;

        if (delta > 0)
            customer.AddLoyaltyPoints(delta);
        else
            customer.RedeemLoyaltyPoints(Math.Min(-delta, customer.LoyaltyPoints));
    }

    /// <summary>Net points earned on a sale that haven't already been reversed.</summary>
    public static int NetEarned(IEnumerable<RewardPointsEntry> txnEntries) =>
        Math.Max(0,
            txnEntries.Where(e => e.EntryType == RewardPointsEntryType.PurchaseEarned).Sum(e => e.Points)
            + txnEntries.Where(e => e.EntryType == RewardPointsEntryType.Reversed).Sum(e => e.Points));

    /// <summary>Net points redeemed on a sale that haven't already been given back.</summary>
    public static int NetRedeemed(IEnumerable<RewardPointsEntry> txnEntries) =>
        Math.Max(0,
            -txnEntries.Where(e => e.EntryType == RewardPointsEntryType.Redeemed).Sum(e => e.Points)
            - txnEntries.Where(e => e.EntryType == RewardPointsEntryType.Refunded).Sum(e => e.Points));
}
