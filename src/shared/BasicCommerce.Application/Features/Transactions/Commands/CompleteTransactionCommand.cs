using BasicCommerce.Application.Features.RewardPoints;
using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record CompleteTransactionCommand(Guid TenantId, Guid TransactionId)
    : IRequest<TransactionResponse>;

public class CompleteTransactionCommandHandler
    : IRequestHandler<CompleteTransactionCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;

    public CompleteTransactionCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TransactionResponse> Handle(
        CompleteTransactionCommand request, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var transaction = await _uow.Transactions.GetWithItemsAsync(
                request.TenantId, request.TransactionId, ct)
                ?? throw new NotFoundException("Transaction", request.TransactionId);

            transaction.Complete();

            await RecordSaleMovementsAsync(transaction, request.TenantId, ct);
            await AwardLoyaltyPointsAsync(transaction, request.TenantId, ct);

            _uow.Transactions.Update(transaction);
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            string? customerName = null;
            if (transaction.CustomerId.HasValue)
            {
                var customer = await _uow.Customers.GetByIdAsync(transaction.CustomerId.Value, ct);
                customerName = customer?.Name;
            }

            return CreateTransactionCommandHandler.MapToResponse(transaction, customerName);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task RecordSaleMovementsAsync(Transaction transaction,
        Guid tenantId, CancellationToken ct)
    {
        foreach (var item in transaction.LineItems.Where(l => !l.IsVoided))
        {
            var stock = await _uow.StockLevels.GetAsync(
                tenantId, transaction.StoreId, item.ProductId, ct);

            if (stock is null) continue;

            var before = stock.Quantity;
            stock.Decrement(item.Quantity);
            _uow.StockLevels.Update(stock);

            var movement = StockMovement.Create(
                tenantId, transaction.StoreId, item.ProductId,
                StockMovementType.Sale, -item.Quantity, before,
                transaction.CashierId,
                reference: transaction.TransactionNumber,
                notes: $"Sale: {item.ProductName}");
            await _uow.StockMovements.AddAsync(movement, ct);

            var product = await _uow.Products.GetByIdAsync(item.ProductId, ct);
            if (product?.IsPerishable == true)
                await DeductBatchesFEFOAsync(tenantId, transaction.StoreId,
                    item.ProductId, item.Quantity, ct);
        }
    }

    private async Task DeductBatchesFEFOAsync(Guid tenantId, Guid storeId,
        Guid productId, decimal quantity, CancellationToken ct)
    {
        var batches = await _uow.StockBatches.GetActiveBatchesFEFOAsync(
            tenantId, storeId, productId, ct);

        var remaining = quantity;
        foreach (var batch in batches)
        {
            if (remaining <= 0) break;
            var consumed = batch.Consume(remaining);
            remaining -= consumed;
            _uow.StockBatches.Update(batch);
        }
    }

    private async Task AwardLoyaltyPointsAsync(Transaction transaction,
        Guid tenantId, CancellationToken ct)
    {
        if (!transaction.CustomerId.HasValue) return;

        var settings = await _uow.RewardPointsSettings.GetByTenantAsync(tenantId, ct);
        if (settings is null || settings.Status != EntityStatus.Active) return;

        // Points are earned on what the customer actually spent — not on the part
        // they paid for with points.
        var paidWithPoints = transaction.Payments
            .Where(p => p.Method == PaymentMethod.RewardPoints && p.PaymentStatus == PaymentStatus.Approved)
            .Sum(p => p.Amount);
        var points = settings.CalculatePurchasePoints(Math.Max(0, transaction.Total - paidWithPoints));
        if (points <= 0) return;

        var customerId = transaction.CustomerId.Value;
        var account = await RewardPointsLedger.GetAccountAsync(
            _uow, settings, tenantId, customerId, transaction.StoreId, ct);

        if (account is null)
        {
            account = RewardPointsAccount.Create(tenantId, customerId,
                RewardPointsLedger.AccountStoreId(settings, transaction.StoreId));
            await _uow.RewardPointsAccounts.AddAsync(account, ct);
        }
        else
        {
            await RewardPointsLedger.ApplyExpiryAsync(_uow, account, ct);
        }

        account.EarnPoints(points, RewardPointsEntryType.PurchaseEarned,
            settings.ActivatePointsImmediately,
            settings.PurchasePointsValidityDays,
            transaction.Id,
            $"Purchase: {transaction.TransactionNumber}");

        // Pending (not yet activated) points aren't spendable, so don't count them yet.
        if (settings.ActivatePointsImmediately)
            await RewardPointsLedger.AdjustCustomerSummaryAsync(_uow, customerId, points, ct);
    }
}
