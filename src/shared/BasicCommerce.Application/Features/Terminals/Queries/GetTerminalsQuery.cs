using BasicCommerce.Application.Features.Terminals;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Stores;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Terminals.Queries;

public record GetAllTerminalsQuery : IRequest<IEnumerable<TerminalResponse>>;
public record GetTerminalQuery(Guid TerminalId) : IRequest<TerminalResponse>;

public class GetAllTerminalsQueryHandler : IRequestHandler<GetAllTerminalsQuery, IEnumerable<TerminalResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetAllTerminalsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<TerminalResponse>> Handle(GetAllTerminalsQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var terminals = await _uow.Terminals.GetAllForTenantAsync(tenantId, ct);
        var stores = await _uow.Stores.GetAllForTenantAsync(tenantId, ct);
        var storeNames = stores.ToDictionary(s => s.Id, s => s.Name);

        return terminals.Select(t => TerminalMapper.ToResponse(t, storeNames.GetValueOrDefault(t.StoreId, "")));
    }
}

public class GetTerminalQueryHandler : IRequestHandler<GetTerminalQuery, TerminalResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetTerminalQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TerminalResponse> Handle(GetTerminalQuery request, CancellationToken ct)
    {
        var terminal = await _uow.Terminals.GetByIdAsync(request.TerminalId, ct)
            ?? throw new NotFoundException("Terminal", request.TerminalId);
        if (terminal.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Terminal", request.TerminalId);

        var store = await _uow.Stores.GetByIdAsync(terminal.StoreId, ct);
        return TerminalMapper.ToResponse(terminal, store?.Name ?? "");
    }
}
