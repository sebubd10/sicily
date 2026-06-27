using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Customers.Commands;

public record DeleteCustomerCommand(Guid CustomerId) : IRequest;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteCustomerCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteCustomerCommand request, CancellationToken ct)
    {
        var customer = await _uow.Customers.GetByIdAsync(request.CustomerId, ct)
            ?? throw new NotFoundException("Customer", request.CustomerId);

        if (customer.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Customer", request.CustomerId);

        customer.SoftDelete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}
