using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Inventory;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Inventory.Queries;

public record GetExpiringBatchesQuery(
    Guid StoreId,
    int WithinDays = 30) : IRequest<IReadOnlyList<StockBatchResponse>>;

public class GetExpiringBatchesQueryHandler
    : IRequestHandler<GetExpiringBatchesQuery, IReadOnlyList<StockBatchResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetExpiringBatchesQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<StockBatchResponse>> Handle(
        GetExpiringBatchesQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);
        if (store.TenantId != tenantId) throw new NotFoundException("Store", request.StoreId);

        var batches = await _uow.StockBatches.GetExpiringAsync(
            tenantId, request.StoreId, request.WithinDays, ct);

        return batches.Select(b => BatchMapper.ToResponse(b, b.Product!)).ToList();
    }
}
