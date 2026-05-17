using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Warehouses;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Warehouses.Queries;

public record GetWarehousesQuery : IRequest<IEnumerable<WarehouseResponse>>;
public record GetWarehouseQuery(Guid WarehouseId) : IRequest<WarehouseResponse>;
public record GetWarehouseStockQuery(Guid WarehouseId) : IRequest<IEnumerable<WarehouseStockLevelResponse>>;
public record GetWarehouseMovementsQuery(Guid WarehouseId, DateTime? From = null, DateTime? To = null, int Limit = 200)
    : IRequest<IEnumerable<WarehouseMovementResponse>>;

public class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, IEnumerable<WarehouseResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetWarehousesQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<WarehouseResponse>> Handle(GetWarehousesQuery request, CancellationToken ct)
    {
        var all = await _uow.Warehouses.GetAllForTenantAsync(_currentUser.TenantId, ct);
        return all.Select(WarehouseMapper.ToResponse);
    }
}

public class GetWarehouseQueryHandler : IRequestHandler<GetWarehouseQuery, WarehouseResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetWarehouseQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<WarehouseResponse> Handle(GetWarehouseQuery request, CancellationToken ct)
    {
        var warehouse = await _uow.Warehouses.GetByIdAsync(request.WarehouseId, ct)
            ?? throw new NotFoundException("Warehouse", request.WarehouseId);
        if (warehouse.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Warehouse", request.WarehouseId);
        return WarehouseMapper.ToResponse(warehouse);
    }
}

public class GetWarehouseStockQueryHandler : IRequestHandler<GetWarehouseStockQuery, IEnumerable<WarehouseStockLevelResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetWarehouseStockQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<WarehouseStockLevelResponse>> Handle(GetWarehouseStockQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var warehouse = await _uow.Warehouses.GetByIdAsync(request.WarehouseId, ct)
            ?? throw new NotFoundException("Warehouse", request.WarehouseId);
        if (warehouse.TenantId != tenantId) throw new NotFoundException("Warehouse", request.WarehouseId);

        var stock = await _uow.WarehouseStockLevels.GetByWarehouseAsync(tenantId, request.WarehouseId, ct);
        return stock.Select(s => WarehouseMapper.ToStockResponse(s, warehouse.Name));
    }
}

public class GetWarehouseMovementsQueryHandler : IRequestHandler<GetWarehouseMovementsQuery, IEnumerable<WarehouseMovementResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetWarehouseMovementsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<WarehouseMovementResponse>> Handle(GetWarehouseMovementsQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var warehouse = await _uow.Warehouses.GetByIdAsync(request.WarehouseId, ct)
            ?? throw new NotFoundException("Warehouse", request.WarehouseId);
        if (warehouse.TenantId != tenantId) throw new NotFoundException("Warehouse", request.WarehouseId);

        var movements = await _uow.WarehouseMovements.GetByWarehouseAsync(
            tenantId, request.WarehouseId, request.From, request.To, request.Limit, ct);
        return movements.Select(WarehouseMapper.ToMovementResponse);
    }
}
