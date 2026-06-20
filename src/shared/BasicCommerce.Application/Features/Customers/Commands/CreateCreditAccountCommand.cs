using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Customers;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Customers.Commands;

public record CreateCreditAccountCommand(Guid CustomerId, Guid StoreId, decimal CreditLimit)
    : IRequest<CreditAccountSummary>;

public class CreateCreditAccountCommandValidator : AbstractValidator<CreateCreditAccountCommand>
{
    public CreateCreditAccountCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.CreditLimit).GreaterThan(0);
    }
}

public class CreateCreditAccountCommandHandler : IRequestHandler<CreateCreditAccountCommand, CreditAccountSummary>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateCreditAccountCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<CreditAccountSummary> Handle(CreateCreditAccountCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var customer = await _uow.Customers.GetByIdAsync(request.CustomerId, ct)
            ?? throw new NotFoundException("Customer", request.CustomerId);
        if (customer.TenantId != tenantId)
            throw new NotFoundException("Customer", request.CustomerId);

        var store = await _uow.Stores.GetByIdForTenantAsync(tenantId, request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);

        var existing = await _uow.CreditAccounts.GetByCustomerAndStoreAsync(
            tenantId, customer.Id, store.Id, ct);
        if (existing is not null)
            throw new DomainException(
                $"A credit account for '{customer.Name}' at '{store.Name}' already exists.");

        var account = CreditAccount.Create(tenantId, customer.Id, store.Id, request.CreditLimit);
        await _uow.CreditAccounts.AddAsync(account, ct);

        // Keep customer's credit limit in sync
        if (request.CreditLimit > customer.CreditLimit)
            customer.UpdateCreditLimit(request.CreditLimit);

        await _uow.SaveChangesAsync(ct);

        return new CreditAccountSummary(
            account.Id,
            customer.Id,
            customer.Name,
            customer.Code,
            store.Name,
            account.CreditLimit,
            account.OutstandingBalance,
            account.AvailableCredit,
            account.Status.ToString(),
            account.LastPaymentAt,
            account.CreatedAt);
    }
}
