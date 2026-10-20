using BasicCommerce.Application.Features.RewardPoints;
using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record ReturnItem(
    Guid OriginalLineItemId,
    decimal Quantity,
    ReturnReason Reason,
    DamageDisposition Disposition);

public record CreateReturnTransactionCommand(
    Guid TenantId,
    Guid OriginalTransactionId,
    Guid? TerminalId,
    IEnumerable<ReturnItem> Items,
    PaymentMethod RefundMethod,
    string? Notes = null) : IRequest<TransactionResponse>;

public class CreateReturnTransactionCommandValidator
    : AbstractValidator<CreateReturnTransactionCommand>
{
    public CreateReturnTransactionCommandValidator()
    {
        RuleFor(x => x.OriginalTransactionId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item must be returned.");
        RuleForEach(x => x.Items).ChildRules(item =>
            item.RuleFor(i => i.Quantity).GreaterThan(0));
    }
}

public class CreateReturnTransactionCommandHandler
    : IRequestHandler<CreateReturnTransactionCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;

    public CreateReturnTransactionCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TransactionResponse> Handle(
        CreateReturnTransactionCommand request, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            return await HandleInternalAsync(request, ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task<TransactionResponse> HandleInternalAsync(
        CreateReturnTransactionCommand request, CancellationToken ct)
    {
        var original = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.OriginalTransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.OriginalTransactionId);

        if (original.TransactionStatus != TransactionStatus.Completed)
            throw new DomainException("Only completed transactions can be returned.");

        var returnItems = request.Items.ToList();
        var terminalId = request.TerminalId ?? original.TerminalId;

        var returnTx = Transaction.CreateReturn(
            request.TenantId, original.StoreId, terminalId,
            original.CashierId, original.Id, original.CustomerId);

        await _uow.Transactions.AddAsync(returnTx, ct);
        await _uow.SaveChangesAsync(ct);

        foreach (var returnItem in returnItems)
        {
            var originalLine = original.LineItems
                .FirstOrDefault(l => l.Id == returnItem.OriginalLineItemId)
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

            var stockLevel = await _uow.StockLevels.GetAsync(
                request.TenantId, original.StoreId, originalLine.ProductId, ct);

            if (stockLevel is not null)
            {
                var before = stockLevel.Quantity;

                if (returnItem.Disposition == DamageDisposition.RestoreToStock)
                {
                    stockLevel.Increment(returnItem.Quantity);
                    _uow.StockLevels.Update(stockLevel);

                    await _uow.StockMovements.AddAsync(StockMovement.Create(
                        request.TenantId, original.StoreId, originalLine.ProductId,
                        StockMovementType.Return, returnItem.Quantity, before,
                        original.CashierId,
                        reference: original.TransactionNumber,
                        notes: $"Return ({returnItem.Reason}): {originalLine.ProductName}"), ct);
                }
                else
                {
                    // Damaged — write off rather than restoring to saleable stock
                    if (stockLevel.Quantity >= returnItem.Quantity)
                    {
                        stockLevel.Decrement(returnItem.Quantity);
                        _uow.StockLevels.Update(stockLevel);
                    }

                    await _uow.StockMovements.AddAsync(StockMovement.Create(
                        request.TenantId, original.StoreId, originalLine.ProductId,
                        StockMovementType.WriteOff, -returnItem.Quantity, before,
                        original.CashierId,
                        reference: original.TransactionNumber,
                        notes: $"Damage write-off ({returnItem.Reason}): {originalLine.ProductName}"), ct);
                }
            }
        }

        var returnTotal = returnTx.Total;

        var refund = returnTx.AddPayment(request.RefundMethod, returnTotal,
            $"Refund for {original.TransactionNumber}");
        refund.Approve();
        returnTx.RefreshAmountPaid();
        returnTx.Complete();

        await SettleRewardPointsAsync(request, original, returnTx, returnTotal, ct);

        original.MarkRefunded();
        _uow.Transactions.Update(original);
        _uow.Transactions.Update(returnTx);
        await _uow.SaveChangesAsync(ct);
        await _uow.CommitTransactionAsync(ct);

        string? customerName = null;
        if (original.CustomerId.HasValue)
        {
            var customer = await _uow.Customers.GetByIdAsync(original.CustomerId.Value, ct);
            customerName = customer?.Name;
        }

        return CreateTransactionCommandHandler.MapToResponse(returnTx, customerName);
    }

    /// <summary>
    /// Takes back the points earned on the returned portion of the sale, and — when the
    /// refund is paid in points — credits the refund to the customer's points balance.
    /// </summary>
    private async Task SettleRewardPointsAsync(CreateReturnTransactionCommand request,
        Domain.Entities.Transaction original, Domain.Entities.Transaction returnTx,
        decimal returnTotal, CancellationToken ct)
    {
        var refundInPoints = request.RefundMethod == PaymentMethod.RewardPoints;

        if (!original.CustomerId.HasValue)
        {
            if (refundInPoints)
                throw new DomainException("A refund in reward points needs a customer on the original sale.");
            return;
        }

        var settings = await _uow.RewardPointsSettings.GetByTenantAsync(request.TenantId, ct);
        if (settings is null)
        {
            if (refundInPoints)
                throw new DomainException("Reward points programme is not configured.");
            return;
        }

        var customerId = original.CustomerId.Value;
        var account = await RewardPointsLedger.GetAccountAsync(
            _uow, settings, request.TenantId, customerId, original.StoreId, ct);

        if (account is not null && original.Total > 0)
        {
            var entries = await _uow.RewardPointsAccounts.GetEntriesForTransactionAsync(original.Id, ct);
            var earned = RewardPointsLedger.NetEarned(entries);
            var share = Math.Min(1m, returnTotal / original.Total);
            var toReverse = (int)Math.Ceiling(earned * share);

            if (toReverse > 0)
            {
                var reversed = account.ReversePoints(toReverse, original.Id,
                    $"Return {returnTx.TransactionNumber}");
                await RewardPointsLedger.AdjustCustomerSummaryAsync(_uow, customerId, -reversed, ct);
            }
        }

        if (refundInPoints)
        {
            if (settings.ExchangeRate <= 0)
                throw new DomainException("Invalid reward points exchange rate.");

            if (account is null)
            {
                account = RewardPointsAccount.Create(request.TenantId, customerId,
                    RewardPointsLedger.AccountStoreId(settings, original.StoreId));
                await _uow.RewardPointsAccounts.AddAsync(account, ct);
            }

            var credit = (int)Math.Floor(returnTotal / settings.ExchangeRate);
            account.CreditRefund(credit, returnTx.Id, $"Refund for {original.TransactionNumber}");
            await RewardPointsLedger.AdjustCustomerSummaryAsync(_uow, customerId, credit, ct);
        }
    }
}
