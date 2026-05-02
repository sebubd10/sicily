using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record AddPaymentCommand(
    Guid TenantId,
    Guid TransactionId,
    PaymentMethod Method,
    decimal Amount,
    string? MobileNumber = null,
    string? Reference = null) : IRequest<TransactionResponse>;

public class AddPaymentCommandValidator : AbstractValidator<AddPaymentCommand>
{
    public AddPaymentCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.MobileNumber).NotEmpty()
            .When(x => x.Method is PaymentMethod.BKash or PaymentMethod.Nagad or PaymentMethod.Rocket)
            .WithMessage("Mobile number is required for mobile wallet payments.");
    }
}

public class AddPaymentCommandHandler : IRequestHandler<AddPaymentCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;

    public AddPaymentCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TransactionResponse> Handle(AddPaymentCommand request, CancellationToken ct)
    {
        var transaction = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.TransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.TransactionId);

        var payment = transaction.AddPayment(request.Method, request.Amount, request.Reference);

        if (request.MobileNumber is not null)
            payment.SetMobileNumber(request.MobileNumber);

        switch (request.Method)
        {
            case PaymentMethod.Cash:
                payment.Approve();
                break;

            case PaymentMethod.Credit:
                await ChargeCreditAccountAsync(transaction, request, ct);
                payment.Approve();
                break;

            // Card / mobile wallet: mark pending — external gateway approves
        }

        transaction.RefreshAmountPaid();
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

    private async Task ChargeCreditAccountAsync(
        Domain.Entities.Transaction transaction,
        AddPaymentCommand request, CancellationToken ct)
    {
        if (!transaction.CustomerId.HasValue)
            throw new DomainException("A customer must be attached for credit sales.");

        var creditAccount = await _uow.CreditAccounts
            .GetByCustomerAndStoreAsync(request.TenantId,
                transaction.CustomerId.Value, transaction.StoreId, ct)
            ?? throw new DomainException(
                "Customer has no credit account at this store.");

        creditAccount.Charge(transaction.Id, request.Amount,
            $"Sale: {transaction.TransactionNumber}");

        var customer = await _uow.Customers.GetByIdAsync(transaction.CustomerId.Value, ct);
        customer?.AddToBalance(request.Amount);
    }
}
