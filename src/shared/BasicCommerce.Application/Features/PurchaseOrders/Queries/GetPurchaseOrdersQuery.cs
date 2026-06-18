using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.PurchaseOrders;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.PurchaseOrders.Queries;

public record GetPurchaseOrdersQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? SupplierId = null,
    Guid? WarehouseId = null,
    PurchaseOrderStatus? Status = null,
    DateTime? From = null,
    DateTime? To = null,
    string? DateField = null) : IRequest<PurchaseOrderListResponse>;

public record GetPurchaseOrderQuery(Guid PurchaseOrderId) : IRequest<PurchaseOrderResponse>;

public class GetPurchaseOrdersQueryHandler : IRequestHandler<GetPurchaseOrdersQuery, PurchaseOrderListResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetPurchaseOrdersQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<PurchaseOrderListResponse> Handle(GetPurchaseOrdersQuery request, CancellationToken ct)
    {
        var (items, total) = await _uow.PurchaseOrders.GetPagedAsync(
            _currentUser.TenantId, request.Page, request.PageSize,
            request.SupplierId, request.WarehouseId, request.Status,
            request.From, request.To, request.DateField, ct);

        return new PurchaseOrderListResponse(
            items.Select(PurchaseOrderMapper.ToSummary), total, request.Page, request.PageSize);
    }
}

public class GetPurchaseOrderQueryHandler : IRequestHandler<GetPurchaseOrderQuery, PurchaseOrderResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetPurchaseOrderQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<PurchaseOrderResponse> Handle(GetPurchaseOrderQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var po = await _uow.PurchaseOrders.GetWithItemsAsync(tenantId, request.PurchaseOrderId, ct)
            ?? throw new NotFoundException("PurchaseOrder", request.PurchaseOrderId);
        if (po.TenantId != tenantId) throw new NotFoundException("PurchaseOrder", request.PurchaseOrderId);
        return PurchaseOrderMapper.ToResponse(po);
    }
}
