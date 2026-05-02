using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record ReturnItem(Guid OriginalLineItemId, decimal Quantity);

public record CreateReturnTransactionCommand(
    Guid TenantId,
    Guid OriginalTransactionId,
    Guid TerminalId,
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
        var original = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.OriginalTransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.OriginalTransactionId);

        if (original.TransactionStatus != TransactionStatus.Completed)
            throw new DomainException("Only completed transactions can be returned.");

        var returnItems = request.Items.ToList();
        var returnTotal = 0m;

        var returnTx = Transaction.CreateReturn(
            request.TenantId, original.StoreId, request.TerminalId,
            original.CashierId, original.Id, original.CustomerId);

        await _uow.Transactions.AddAsync(returnTx, ct);
        await _uow.SaveChangesAsync(ct);

        foreach (var returnItem in returnItems)
        {
            var originalLine = original.LineItems.FirstOrDefault(l => l.Id == returnItem.OriginalLineItemId)
                ?? throw new DomainException($"Line item {returnItem.OriginalLineItemId} not found in original transaction.");

            if (returnItem.Quantity > originalLine.Quantity)
                throw new DomainException(
                    $"Cannot return {returnItem.Quantity} of '{originalLine.ProductName}' — original quantity was {originalLine.Quantity}.");

            var lineTotal = originalLine.UnitPrice * returnItem.Quantity;
            returnTotal += lineTotal;

            var stockLevel = await _uow.StockLevels.GetAsync(
                request.TenantId, original.StoreId, originalLine.ProductId, ct);

            if (stockLevel is not null)
            {
                var before = stockLevel.Quantity;
                stockLevel.Increment(returnItem.Quantity);
                _uow.StockLevels.Update(stockLevel);

                await _uow.StockMovements.AddAsync(StockMovement.Create(
                    request.TenantId, original.StoreId, originalLine.ProductId,
                    StockMovementType.Return, returnItem.Quantity, before,
                    original.CashierId,
                    reference: original.TransactionNumber,
                    notes: $"Return: {originalLine.ProductName}"), ct);
            }
        }

        // Issue refund payment and complete the return
        var refund = returnTx.AddPayment(request.RefundMethod, returnTotal,
            $"Refund for {original.TransactionNumber}");
        refund.Approve();
        returnTx.RefreshAmountPaid();
        returnTx.Complete();

        // Award negative loyalty points (reversal) if customer attached
        if (original.CustomerId.HasValue)
        {
            var customer = await _uow.Customers.GetByIdAsync(original.CustomerId.Value, ct);
            if (customer is not null)
            {
                var pointsToReverse = (int)(returnTotal / 100);
                if (pointsToReverse > 0 && customer.LoyaltyPoints >= pointsToReverse)
                    customer.RedeemLoyaltyPoints(pointsToReverse);
            }
        }

        _uow.Transactions.Update(returnTx);
        await _uow.SaveChangesAsync(ct);

        string? customerName = null;
        if (original.CustomerId.HasValue)
        {
            var customer = await _uow.Customers.GetByIdAsync(original.CustomerId.Value, ct);
            customerName = customer?.Name;
        }

        return CreateTransactionCommandHandler.MapToResponse(returnTx, customerName);
    }
}
