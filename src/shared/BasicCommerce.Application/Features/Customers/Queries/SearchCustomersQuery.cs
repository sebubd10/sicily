using BasicCommerce.Application.Features.Customers.Commands;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Customers;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Customers.Queries;

public record SearchCustomersQuery(string? Term, int Page = 1, int PageSize = 20)
    : IRequest<IEnumerable<CustomerResponse>>;

public class SearchCustomersQueryHandler
    : IRequestHandler<SearchCustomersQuery, IEnumerable<CustomerResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public SearchCustomersQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<CustomerResponse>> Handle(
        SearchCustomersQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);

        if (!string.IsNullOrWhiteSpace(request.Term))
        {
            var results = await _uow.Customers.SearchAsync(tenantId, request.Term,
                size * page, ct);
            return results
                .Skip((page - 1) * size)
                .Take(size)
                .Select(RegisterCustomerCommandHandler.ToResponse);
        }

        var all = await _uow.Customers.GetAllForTenantAsync(tenantId, ct);
        return all
            .OrderBy(c => c.Name)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(RegisterCustomerCommandHandler.ToResponse);
    }
}
