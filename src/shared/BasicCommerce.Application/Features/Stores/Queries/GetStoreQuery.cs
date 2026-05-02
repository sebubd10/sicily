using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Stores;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Stores.Queries;

public record GetStoreQuery(Guid StoreId) : IRequest<StoreResponse>;

public class GetStoreQueryHandler : IRequestHandler<GetStoreQuery, StoreResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetStoreQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<StoreResponse> Handle(GetStoreQuery request, CancellationToken ct)
    {
        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);

        if (store.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Store", request.StoreId);

        var terminals = await _uow.Terminals.GetByStoreAsync(store.TenantId, store.Id, ct);

        return new StoreResponse(
            store.Id,
            store.Name,
            store.Code,
            $"{store.Address.Line1}, {store.Address.City}",
            store.Phone,
            store.Email,
            store.IsActive,
            terminals.Count());
    }
}
