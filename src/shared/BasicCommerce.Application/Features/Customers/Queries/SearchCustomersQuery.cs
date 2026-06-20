using BasicCommerce.Application.Features.Customers.Commands;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Customers;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Customers.Queries;

public record SearchCustomersQuery(string? Term, int Page = 1, int PageSize = 20)
    : IRequest<CustomerListResponse>;

public class SearchCustomersQueryHandler
    : IRequestHandler<SearchCustomersQuery, CustomerListResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public SearchCustomersQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<CustomerListResponse> Handle(
        SearchCustomersQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);

        var total = await _uow.Customers.GetCountAsync(tenantId, request.Term, ct);
        var items = await _uow.Customers.GetPagedAsync(tenantId, request.Term, page, size, ct);

        return new CustomerListResponse(
            items.Select(RegisterCustomerCommandHandler.ToResponse),
            total, page, size);
    }
}
