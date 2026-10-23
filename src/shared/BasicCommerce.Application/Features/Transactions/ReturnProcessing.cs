using BasicCommerce.Application.Features.RewardPoints;
using BasicCommerce.Application.Features.Transactions.Commands;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;

namespace BasicCommerce.Application.Features.Transactions;

/// <summary>
/// Building a return against a completed sale — shared by plain returns and exchanges so
/// both refund, restock and reverse points by exactly the same rules.
/// </summary>
internal static class ReturnProcessing
{
    /// <summary>
    /// Creates the return transaction with its lines (each refunding its share of the
    /// original discount plus the VAT paid) and puts stock back or writes it off. Doesn't
    /// add refund payments or complete it — the caller does, since how the refund is paid out
    /// differs between a return and an exchange.
    /// </summary>
    public static async Task<Transaction> BuildReturnAsync(IUnitOfWork uow, Transaction original,
        Guid terminalId, IReadOnlyList<ReturnItem> items, CancellationToken ct)
    {
        if (original.TransactionStatus != TransactionStatus.Completed)
            throw new DomainException("Only completed transactions can be returned.");
        if (items.Count == 0)
            throw new DomainException("At least one item must be returned.");

        var returnTx = Transaction.CreateReturn(
            original.TenantId, original.StoreId, terminalId,
            original.CashierId, original.Id, original.CustomerId);
        await uow.Transactions.AddAsync(returnTx, ct);

        foreach (var returnItem in items)
        {
            var originalLine = original.LineItems
                .FirstOrDefault(l => l.Id == returnItem.OriginalLineItemId && !l.IsVoided)
                ?? throw new DomainException(
                    $"Line item {returnItem.OriginalLineItemId} not found in original transaction.");

            if (returnItem.Quantity > originalLine.Quantity)
                throw new DomainException(
                    $"Cannot return {returnItem.Quantity} of '{originalLine.ProductName}' " +
                    $"— original quantity was {originalLine.Quantity}.");

            // Refund exactly what was paid for these units: their share of the original
            // discount, plus the VAT charged on them.
            var discountShare = originalLine.Quantity > 0
                ? Math.Round(originalLine.DiscountAmount * returnItem.Quantity / originalLine.Quantity, 2,
                    MidpointRounding.AwayFromZero)
                : 0m;

            returnTx.AddReturnLineItem(
                originalLine.ProductId, originalLine.ProductName, originalLine.ProductSku,
                returnItem.Quantity, originalLine.UnitPrice,
                returnItem.Reason, returnItem.Disposition,
                discountShare, originalLine.TaxRate);

            var stockLevel = await uow.StockLevels.GetAsync(
                original.TenantId, original.StoreId, originalLine.ProductId, ct);
            if (stockLevel is null) continue;

            var before = stockLevel.Quantity;
            if (returnItem.Disposition == DamageDisposition.RestoreToStock)
            {
                stockLevel.Increment(returnItem.Quantity);
                await uow.StockMovements.AddAsync(StockMovement.Create(
                    original.TenantId, original.StoreId, originalLine.ProductId,
                    StockMovementType.Return, returnItem.Quantity, before,
                    original.CashierId,
                    reference: original.TransactionNumber,
                    notes: $"Return ({returnItem.Reason}): {originalLine.ProductName}"), ct);
            }
            else
            {
                // Damaged — write off rather than restoring to saleable stock
                if (stockLevel.Quantity >= returnItem.Quantity)
                    stockLevel.Decrement(returnItem.Quantity);

                await uow.StockMovements.AddAsync(StockMovement.Create(
                    original.TenantId, original.StoreId, originalLine.ProductId,
                    StockMovementType.WriteOff, -returnItem.Quantity, before,
                    original.CashierId,
                    reference: original.TransactionNumber,
                    notes: $"Damage write-off ({returnItem.Reason}): {originalLine.ProductName}"), ct);
            }
        }

        return returnTx;
    }

    /// <summary>Adds an approved refund payment to the return.</summary>
    public static Payment? AddRefund(Transaction returnTx, PaymentMethod method, decimal amount, string reference)
    {
        if (amount <= 0) return null;
        var refund = returnTx.AddPayment(method, amount, reference);
        refund.Approve();
        returnTx.RefreshAmountPaid();
        return refund;
    }

    /// <summary>
    /// Takes back the points earned on the returned share of the sale, and — when part of the
    /// refund is paid in points — credits that to the customer's balance.
    /// </summary>
    public static async Task SettleRewardPointsAsync(IUnitOfWork uow, Transaction original,
        Transaction returnTx, decimal pointsRefundAmount, CancellationToken ct)
    {
        var returnTotal = returnTx.Total;
        var refundInPoints = pointsRefundAmount > 0;

        if (!original.CustomerId.HasValue)
        {
            if (refundInPoints)
                throw new DomainException("A refund in reward points needs a customer on the original sale.");
            return;
        }

        var settings = await uow.RewardPointsSettings.GetByTenantAsync(original.TenantId, ct);
        if (settings is null)
        {
            if (refundInPoints)
                throw new DomainException("Reward points programme is not configured.");
            return;
        }

        var customerId = original.CustomerId.Value;
        var account = await RewardPointsLedger.GetAccountAsync(
            uow, settings, original.TenantId, customerId, original.StoreId, ct);

        if (account is not null && original.Total > 0)
        {
            var entries = await uow.RewardPointsAccounts.GetEntriesForTransactionAsync(original.Id, ct);
            var earned = RewardPointsLedger.NetEarned(entries);
            var share = Math.Min(1m, returnTotal / original.Total);
            var toReverse = (int)Math.Ceiling(earned * share);

            if (toReverse > 0)
            {
                var reversed = account.ReversePoints(toReverse, original.Id,
                    $"Return {returnTx.TransactionNumber}");
                await RewardPointsLedger.AdjustCustomerSummaryAsync(uow, customerId, -reversed, ct);
            }
        }

        if (refundInPoints)
        {
            if (settings.ExchangeRate <= 0)
                throw new DomainException("Invalid reward points exchange rate.");

            if (account is null)
            {
                account = RewardPointsAccount.Create(original.TenantId, customerId,
                    RewardPointsLedger.AccountStoreId(settings, original.StoreId));
                await uow.RewardPointsAccounts.AddAsync(account, ct);
            }

            var credit = (int)Math.Floor(pointsRefundAmount / settings.ExchangeRate);
            account.CreditRefund(credit, returnTx.Id, $"Refund for {original.TransactionNumber}");
            await RewardPointsLedger.AdjustCustomerSummaryAsync(uow, customerId, credit, ct);
        }
    }
}
