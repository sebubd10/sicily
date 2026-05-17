using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Customers;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Customers.Commands;

public record UpdateCreditLimitCommand(Guid CustomerId, decimal CreditLimit) : IRequest<CustomerResponse>;

public class UpdateCreditLimitCommandValidator : AbstractValidator<UpdateCreditLimitCommand>
{
    public UpdateCreditLimitCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0);
    }
}

public class UpdateCreditLimitCommandHandler : IRequestHandler<UpdateCreditLimitCommand, CustomerResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateCreditLimitCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<CustomerResponse> Handle(UpdateCreditLimitCommand request, CancellationToken ct)
    {
        var customer = await _uow.Customers.GetByIdAsync(request.CustomerId, ct)
            ?? throw new NotFoundException("Customer", request.CustomerId);

        if (customer.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Customer", request.CustomerId);

        customer.UpdateCreditLimit(request.CreditLimit);

        var creditAccounts = await _uow.CreditAccounts.GetByCustomerAsync(
            _currentUser.TenantId, customer.Id, ct);
        foreach (var account in creditAccounts)
            account.UpdateCreditLimit(request.CreditLimit);

        await _uow.SaveChangesAsync(ct);
        return RegisterCustomerCommandHandler.ToResponse(customer);
    }
}
