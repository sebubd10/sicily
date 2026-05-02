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

    private const int LoyaltyPointsPerBdt = 100;

    public CompleteTransactionCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TransactionResponse> Handle(
        CompleteTransactionCommand request, CancellationToken ct)
    {
        var transaction = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.TransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.TransactionId);

        transaction.Complete();

        await RecordSaleMovementsAsync(transaction, request.TenantId, ct);
        await AwardLoyaltyPointsAsync(transaction, request.TenantId, ct);

        _uow.Transactions.Update(transaction);
        await _uow.SaveChangesAsync(ct);

        string? customerName = null;
        if (transaction.CustomerId.HasValue)
        {
            var customer = await _uow.Customers.GetByIdAsync(transaction.CustomerId.Value, ct);
            customerName = customer?.Name;
        }

        return CreateTransactionCommandHandler.MapToResponse(transaction, customerName);
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
        }
    }

    private async Task AwardLoyaltyPointsAsync(Transaction transaction,
        Guid tenantId, CancellationToken ct)
    {
        if (!transaction.CustomerId.HasValue) return;

        var customer = await _uow.Customers.GetByIdAsync(transaction.CustomerId.Value, ct);
        if (customer is null) return;

        var points = (int)(transaction.Total / LoyaltyPointsPerBdt);
        if (points > 0)
        {
            customer.AddLoyaltyPoints(points);
            _uow.Customers.Update(customer);
        }
    }
}
