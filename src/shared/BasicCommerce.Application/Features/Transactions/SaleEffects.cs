using BasicCommerce.Application.Features.RewardPoints;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;

namespace BasicCommerce.Application.Features.Transactions;

/// <summary>
/// Side effects a sale has outside the transaction itself — stock, gift cards, credit
/// accounts, reward points — and how to undo them. Shared by complete, void, payment
/// removal and exchange so each path stays consistent.
/// </summary>
internal static class SaleEffects
{
    /// <summary>Takes the sold quantities out of stock (FEFO batches for perishables).</summary>
    public static async Task DeductStockAsync(IUnitOfWork uow, Transaction transaction, CancellationToken ct)
    {
        foreach (var item in transaction.LineItems.Where(l => !l.IsVoided))
        {
            var stock = await uow.StockLevels.GetAsync(
                transaction.TenantId, transaction.StoreId, item.ProductId, ct);
            if (stock is null) continue;

            var before = stock.Quantity;
            stock.Decrement(item.Quantity);

            await uow.StockMovements.AddAsync(StockMovement.Create(
                transaction.TenantId, transaction.StoreId, item.ProductId,
                StockMovementType.Sale, -item.Quantity, before,
                transaction.CashierId,
                reference: transaction.TransactionNumber,
                notes: $"Sale: {item.ProductName}"), ct);

            var product = await uow.Products.GetByIdAsync(item.ProductId, ct);
            if (product?.IsPerishable == true)
                await DeductBatchesFefoAsync(uow, transaction, item.ProductId, item.Quantity, ct);
        }
    }

    /// <summary>Puts back the stock a completed sale took (used when it's voided).</summary>
    /// <remarks>
    /// Restored to the store's stock level only; perishable batch quantities consumed by the
    /// sale are not re-credited to specific batches.
    /// </remarks>
    public static async Task RestoreStockAsync(IUnitOfWork uow, Transaction transaction,
        Guid userId, CancellationToken ct)
    {
        foreach (var item in transaction.LineItems.Where(l => !l.IsVoided))
        {
            var stock = await uow.StockLevels.GetAsync(
                transaction.TenantId, transaction.StoreId, item.ProductId, ct);
            if (stock is null) continue;

            var before = stock.Quantity;
            stock.Increment(item.Quantity);

            await uow.StockMovements.AddAsync(StockMovement.Create(
                transaction.TenantId, transaction.StoreId, item.ProductId,
                StockMovementType.SaleVoided, item.Quantity, before, userId,
                reference: transaction.TransactionNumber,
                notes: $"Sale voided: {item.ProductName}"), ct);
        }
    }

    /// <summary>
    /// Undoes what taking a payment did outside the sale: gives back gift card balance,
    /// cancels the credit charge, restores redeemed points. Cash/card/wallet payments have
    /// nothing to undo here — the cashier hands that money back.
    /// </summary>
    public static async Task ReversePaymentAsync(IUnitOfWork uow, Transaction transaction,
        Payment payment, string note, CancellationToken ct)
    {
        switch (payment.Method)
        {
            case PaymentMethod.GiftCard:
            {
                var code = payment.GatewayReference
                    ?? throw new DomainException("Gift card payment has no card code recorded.");
                var card = await uow.GiftCards.GetByCodeAsync(transaction.TenantId, code, ct)
                    ?? throw new DomainException($"Gift card '{code}' not found.");
                card.RefundRedemption(payment.Amount, transaction.Id, note);
                break;
            }

            case PaymentMethod.Credit when transaction.CustomerId.HasValue:
            {
                var account = await uow.CreditAccounts.GetByCustomerAndStoreAsync(
                    transaction.TenantId, transaction.CustomerId.Value, transaction.StoreId, ct);
                account?.ReverseCharge(transaction.Id, payment.Amount, note);

                var customer = await uow.Customers.GetByIdAsync(transaction.CustomerId.Value, ct);
                customer?.ReduceBalance(payment.Amount);
                break;
            }

            case PaymentMethod.RewardPoints when transaction.CustomerId.HasValue:
            {
                var settings = await uow.RewardPointsSettings.GetByTenantAsync(transaction.TenantId, ct);
                if (settings is null || settings.ExchangeRate <= 0) break;

                var account = await RewardPointsLedger.GetAccountAsync(uow, settings,
                    transaction.TenantId, transaction.CustomerId.Value, transaction.StoreId, ct);
                if (account is null) break;

                var points = (int)Math.Ceiling(payment.Amount / settings.ExchangeRate);
                var restored = account.RestoreRedeemedPoints(points, transaction.Id, note);
                await RewardPointsLedger.AdjustCustomerSummaryAsync(
                    uow, transaction.CustomerId.Value, restored, ct);
                break;
            }
        }
    }

    private static async Task DeductBatchesFefoAsync(IUnitOfWork uow, Transaction transaction,
        Guid productId, decimal quantity, CancellationToken ct)
    {
        var batches = await uow.StockBatches.GetActiveBatchesFEFOAsync(
            transaction.TenantId, transaction.StoreId, productId, ct);

        var remaining = quantity;
        foreach (var batch in batches)
        {
            if (remaining <= 0) break;
            remaining -= batch.Consume(remaining);
        }
    }
}
