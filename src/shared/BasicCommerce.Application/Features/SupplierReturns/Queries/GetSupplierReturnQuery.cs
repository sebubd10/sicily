using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.SupplierReturns;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.SupplierReturns.Queries;

public record GetSupplierReturnQuery(Guid Id) : IRequest<SupplierReturnResponse>;

public class GetSupplierReturnQueryHandler
    : IRequestHandler<GetSupplierReturnQuery, SupplierReturnResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetSupplierReturnQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<SupplierReturnResponse> Handle(
        GetSupplierReturnQuery request, CancellationToken ct)
    {
        var sr = await _uow.SupplierReturns.GetWithItemsAsync(
            _currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("SupplierReturn", request.Id);

        return SupplierReturnMapper.ToResponse(sr);
    }
}
