using BasicCommerce.Application.Features.Stores;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Stores;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Stores.Queries;

public record GetStoresQuery : IRequest<IEnumerable<StoreResponse>>;

public class GetStoresQueryHandler : IRequestHandler<GetStoresQuery, IEnumerable<StoreResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetStoresQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<StoreResponse>> Handle(GetStoresQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var stores = await _uow.Stores.GetAllForTenantAsync(tenantId, ct);

        var result = new List<StoreResponse>();
        foreach (var store in stores)
        {
            var terminals = await _uow.Terminals.GetByStoreAsync(tenantId, store.Id, ct);
            result.Add(StoreMapper.ToResponse(store, terminals.Count()));
        }

        return result;
    }
}
