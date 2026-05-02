using BasicCommerce.Application.Features.Customers.Commands;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Customers;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Customers.Queries;

public record GetCustomerQuery(Guid CustomerId) : IRequest<CustomerResponse>;

public class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, CustomerResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetCustomerQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<CustomerResponse> Handle(GetCustomerQuery request, CancellationToken ct)
    {
        var customer = await _uow.Customers.GetByIdAsync(request.CustomerId, ct)
            ?? throw new NotFoundException("Customer", request.CustomerId);

        if (customer.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Customer", request.CustomerId);

        return RegisterCustomerCommandHandler.ToResponse(customer);
    }
}
