using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Inventory;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Inventory.Queries;

public record GetStockBatchesQuery(
    Guid StoreId,
    Guid? ProductId,
    bool IncludeExpired,
    int Page,
    int PageSize) : IRequest<StockBatchListResponse>;

public class GetStockBatchesQueryHandler : IRequestHandler<GetStockBatchesQuery, StockBatchListResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetStockBatchesQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<StockBatchListResponse> Handle(GetStockBatchesQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);
        if (store.TenantId != tenantId) throw new NotFoundException("Store", request.StoreId);

        var batches = await _uow.StockBatches.GetPagedAsync(
            tenantId, request.StoreId, request.ProductId,
            request.IncludeExpired, request.Page, request.PageSize, ct);

        var total = await _uow.StockBatches.GetTotalCountAsync(
            tenantId, request.StoreId, request.ProductId,
            request.IncludeExpired, ct);

        var items = batches
            .Select(b => BatchMapper.ToResponse(b, b.Product!))
            .ToList();

        return new StockBatchListResponse(items, total, request.Page, request.PageSize);
    }
}
