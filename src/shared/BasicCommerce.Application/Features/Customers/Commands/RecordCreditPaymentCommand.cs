using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Customers;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Customers.Commands;

public record RecordCreditPaymentCommand(Guid CreditAccountId, decimal Amount, string? Reference)
    : IRequest<CreditAccountResponse>;

public class RecordCreditPaymentCommandValidator : AbstractValidator<RecordCreditPaymentCommand>
{
    public RecordCreditPaymentCommandValidator()
    {
        RuleFor(x => x.CreditAccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class RecordCreditPaymentCommandHandler
    : IRequestHandler<RecordCreditPaymentCommand, CreditAccountResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public RecordCreditPaymentCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<CreditAccountResponse> Handle(
        RecordCreditPaymentCommand request, CancellationToken ct)
    {
        var account = await _uow.CreditAccounts.GetWithTransactionsAsync(
            _currentUser.TenantId, request.CreditAccountId, ct)
            ?? throw new NotFoundException("CreditAccount", request.CreditAccountId);

        account.RecordPayment(request.Amount, request.Reference);

        var customer = await _uow.Customers.GetByIdAsync(account.CustomerId, ct)!;
        customer?.ReduceBalance(request.Amount);

        var store = await _uow.Stores.GetByIdAsync(account.StoreId, ct);
        await _uow.SaveChangesAsync(ct);

        return GetCreditAccountQueryHandler.ToResponse(account, customer?.Name ?? "Unknown",
            customer?.Code ?? string.Empty, store?.Name ?? string.Empty);
    }
}
