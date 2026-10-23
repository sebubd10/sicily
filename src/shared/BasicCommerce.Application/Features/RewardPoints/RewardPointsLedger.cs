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

    /// <summary>
    /// Awards purchase points for a completed sale with a customer. Points are earned on what
    /// the customer actually spent — not on the part paid for with points.
    /// Returns the points awarded.
    /// </summary>
    public static async Task<int> AwardPurchasePointsAsync(IUnitOfWork uow, Transaction transaction,
        CancellationToken ct)
    {
        if (!transaction.CustomerId.HasValue) return 0;

        var settings = await uow.RewardPointsSettings.GetByTenantAsync(transaction.TenantId, ct);
        if (settings is null || settings.Status != EntityStatus.Active) return 0;

        var paidWithPoints = transaction.Payments
            .Where(p => p.Method == PaymentMethod.RewardPoints && p.PaymentStatus == PaymentStatus.Approved)
            .Sum(p => p.Amount);
        var points = settings.CalculatePurchasePoints(Math.Max(0, transaction.Total - paidWithPoints));
        if (points <= 0) return 0;

        var customerId = transaction.CustomerId.Value;
        var account = await GetAccountAsync(uow, settings, transaction.TenantId, customerId,
            transaction.StoreId, ct);

        if (account is null)
        {
            account = RewardPointsAccount.Create(transaction.TenantId, customerId,
                AccountStoreId(settings, transaction.StoreId));
            await uow.RewardPointsAccounts.AddAsync(account, ct);
        }
        else
        {
            await ApplyExpiryAsync(uow, account, ct);
        }

        account.EarnPoints(points, RewardPointsEntryType.PurchaseEarned,
            settings.ActivatePointsImmediately,
            settings.PurchasePointsValidityDays,
            transaction.Id,
            $"Purchase: {transaction.TransactionNumber}");

        // Pending (not yet activated) points aren't spendable, so don't count them yet.
        if (settings.ActivatePointsImmediately)
            await AdjustCustomerSummaryAsync(uow, customerId, points, ct);

        return points;
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
