using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Inventory;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Inventory.Queries;

public record GetStockLevelsQuery(Guid StoreId) : IRequest<IEnumerable<StockLevelResponse>>;
public record GetLowStockAlertsQuery(Guid StoreId) : IRequest<IEnumerable<StockLevelResponse>>;
public record GetStockLevelQuery(Guid StoreId, Guid ProductId) : IRequest<StockLevelResponse>;

public class GetStockLevelsQueryHandler : IRequestHandler<GetStockLevelsQuery, IEnumerable<StockLevelResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetStockLevelsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<StockLevelResponse>> Handle(GetStockLevelsQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);
        if (store.TenantId != tenantId) throw new NotFoundException("Store", request.StoreId);

        var levels = await _uow.StockLevels.GetByStoreAsync(tenantId, request.StoreId, ct);
        var categoryCache = new Dictionary<Guid, string>();

        var result = new List<StockLevelResponse>();
        foreach (var level in levels)
        {
            if (level.Product is null) continue;
            result.Add(await InventoryMapper.ToResponseWithCategoryAsync(level, level.Product, _uow, categoryCache, ct));
        }
        return result;
    }
}

public class GetLowStockAlertsQueryHandler : IRequestHandler<GetLowStockAlertsQuery, IEnumerable<StockLevelResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetLowStockAlertsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<StockLevelResponse>> Handle(GetLowStockAlertsQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);
        if (store.TenantId != tenantId) throw new NotFoundException("Store", request.StoreId);

        var levels = await _uow.StockLevels.GetLowStockAsync(tenantId, request.StoreId, ct);
        var categoryCache = new Dictionary<Guid, string>();

        var result = new List<StockLevelResponse>();
        foreach (var level in levels)
        {
            if (level.Product is null) continue;
            result.Add(await InventoryMapper.ToResponseWithCategoryAsync(level, level.Product, _uow, categoryCache, ct));
        }
        return result;
    }
}

public class GetStockLevelQueryHandler : IRequestHandler<GetStockLevelQuery, StockLevelResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetStockLevelQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<StockLevelResponse> Handle(GetStockLevelQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var level = await _uow.StockLevels.GetAsync(tenantId, request.StoreId, request.ProductId, ct)
            ?? throw new NotFoundException("StockLevel", request.ProductId);

        var product = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        var category = await _uow.Categories.GetByIdAsync(product.CategoryId, ct);
        return InventoryMapper.ToResponse(level, product, category?.Name ?? string.Empty);
    }
}
