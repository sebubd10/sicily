using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.SupplierReturns;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.SupplierReturns.Queries;

public record GetSupplierReturnsQuery(
    Guid? SupplierId,
    Guid? StoreId,
    SupplierReturnStatus? Status,
    int Page,
    int PageSize) : IRequest<SupplierReturnListResponse>;

public class GetSupplierReturnsQueryHandler
    : IRequestHandler<GetSupplierReturnsQuery, SupplierReturnListResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetSupplierReturnsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<SupplierReturnListResponse> Handle(
        GetSupplierReturnsQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var items = await _uow.SupplierReturns.GetPagedAsync(
            tenantId, request.SupplierId, request.StoreId,
            request.Status, request.Page, request.PageSize, ct);

        var total = await _uow.SupplierReturns.GetTotalCountAsync(
            tenantId, request.SupplierId, request.StoreId, request.Status, ct);

        return new SupplierReturnListResponse(
            items.Select(SupplierReturnMapper.ToSummary).ToList(),
            total, request.Page, request.PageSize);
    }
}
