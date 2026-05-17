using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Customers;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Customers.Commands;

public record AddLoyaltyPointsCommand(Guid CustomerId, int Points) : IRequest<CustomerResponse>;
public record RedeemLoyaltyPointsCommand(Guid CustomerId, int Points) : IRequest<CustomerResponse>;

public class LoyaltyPointsCommandValidator : AbstractValidator<AddLoyaltyPointsCommand>
{
    public LoyaltyPointsCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Points).GreaterThan(0);
    }
}

public class RedeemLoyaltyPointsCommandValidator : AbstractValidator<RedeemLoyaltyPointsCommand>
{
    public RedeemLoyaltyPointsCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Points).GreaterThan(0);
    }
}

public class AddLoyaltyPointsCommandHandler : IRequestHandler<AddLoyaltyPointsCommand, CustomerResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public AddLoyaltyPointsCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<CustomerResponse> Handle(AddLoyaltyPointsCommand request, CancellationToken ct)
    {
        var customer = await _uow.Customers.GetByIdAsync(request.CustomerId, ct)
            ?? throw new NotFoundException("Customer", request.CustomerId);

        if (customer.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Customer", request.CustomerId);

        customer.AddLoyaltyPoints(request.Points);
        await _uow.SaveChangesAsync(ct);
        return RegisterCustomerCommandHandler.ToResponse(customer);
    }
}

public class RedeemLoyaltyPointsCommandHandler : IRequestHandler<RedeemLoyaltyPointsCommand, CustomerResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public RedeemLoyaltyPointsCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<CustomerResponse> Handle(RedeemLoyaltyPointsCommand request, CancellationToken ct)
    {
        var customer = await _uow.Customers.GetByIdAsync(request.CustomerId, ct)
            ?? throw new NotFoundException("Customer", request.CustomerId);

        if (customer.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Customer", request.CustomerId);

        customer.RedeemLoyaltyPoints(request.Points);
        await _uow.SaveChangesAsync(ct);
        return RegisterCustomerCommandHandler.ToResponse(customer);
    }
}
