using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record CompleteTransactionCommand(
    Guid TenantId,
    Guid TransactionId,
    PaymentMethod PaymentMethod,
    decimal AmountTendered,
    string? MobileNumber = null,
    string? GatewayReference = null) : IRequest<TransactionResponse>;

public class CompleteTransactionCommandValidator
    : AbstractValidator<CompleteTransactionCommand>
{
    public CompleteTransactionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.AmountTendered).GreaterThan(0);
        RuleFor(x => x.MobileNumber).NotEmpty()
            .When(x => x.PaymentMethod is PaymentMethod.BKash
                or PaymentMethod.Nagad or PaymentMethod.Rocket)
            .WithMessage("Mobile number is required for mobile wallet payments.");
    }
}

public class CompleteTransactionCommandHandler
    : IRequestHandler<CompleteTransactionCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;

    public CompleteTransactionCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TransactionResponse> Handle(
        CompleteTransactionCommand request, CancellationToken ct)
    {
        var transaction = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.TransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.TransactionId);

        var payment = transaction.AddPayment(
            request.PaymentMethod, request.AmountTendered, request.GatewayReference);

        if (request.MobileNumber is not null)
            payment.SetMobileNumber(request.MobileNumber);

        // For cash payments, approve immediately
        // For card/mobile payments, gateway approval happens externally
        if (request.PaymentMethod == PaymentMethod.Cash)
            payment.Approve();

        if (payment.Status == PaymentStatus.Approved)
        {
            transaction.Complete();
            await DecrementStockAsync(transaction, request.TenantId, ct);
        }

        _uow.Transactions.Update(transaction);
        await _uow.SaveChangesAsync(ct);

        return CreateTransactionCommandHandler.MapToResponse(transaction);
    }

    private async Task DecrementStockAsync(Domain.Entities.Transaction transaction,
        Guid tenantId, CancellationToken ct)
    {
        foreach (var item in transaction.LineItems.Where(l => !l.IsVoided))
        {
            var stock = await _uow.StockLevels.GetAsync(
                tenantId, transaction.StoreId, item.ProductId, ct);

            if (stock is not null)
            {
                stock.Decrement(item.Quantity);
                _uow.StockLevels.Update(stock);
            }
        }
    }
}
