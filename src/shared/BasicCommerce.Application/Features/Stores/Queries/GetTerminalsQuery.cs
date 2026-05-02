using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Stores;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Stores.Queries;

public record GetTerminalsQuery(Guid StoreId) : IRequest<IEnumerable<TerminalResponse>>;

public class GetTerminalsQueryHandler : IRequestHandler<GetTerminalsQuery, IEnumerable<TerminalResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetTerminalsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<TerminalResponse>> Handle(GetTerminalsQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);

        if (store.TenantId != tenantId)
            throw new NotFoundException("Store", request.StoreId);

        var terminals = await _uow.Terminals.GetByStoreAsync(tenantId, request.StoreId, ct);

        return terminals.Select(t => new TerminalResponse(
            t.Id,
            t.Name,
            t.Code,
            t.Type.ToString(),
            t.Status.ToString(),
            null));
    }
}
