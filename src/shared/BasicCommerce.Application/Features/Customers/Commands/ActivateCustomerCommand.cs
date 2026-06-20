using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Customers.Commands;

public record ActivateCustomerCommand(Guid CustomerId) : IRequest;

public class ActivateCustomerCommandHandler : IRequestHandler<ActivateCustomerCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ActivateCustomerCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(ActivateCustomerCommand request, CancellationToken ct)
    {
        var customer = await _uow.Customers.GetByIdAsync(request.CustomerId, ct)
            ?? throw new NotFoundException("Customer", request.CustomerId);

        if (customer.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Customer", request.CustomerId);

        customer.Activate();
        await _uow.SaveChangesAsync(ct);
    }
}
