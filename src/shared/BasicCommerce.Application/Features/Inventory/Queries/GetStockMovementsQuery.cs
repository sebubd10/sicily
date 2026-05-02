using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Inventory;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Inventory.Queries;

public record GetStockMovementsQuery(
    Guid StoreId,
    Guid? ProductId = null,
    DateTime? From = null,
    DateTime? To = null,
    int Limit = 100) : IRequest<IEnumerable<StockMovementResponse>>;

public class GetStockMovementsQueryHandler
    : IRequestHandler<GetStockMovementsQuery, IEnumerable<StockMovementResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetStockMovementsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<StockMovementResponse>> Handle(
        GetStockMovementsQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);
        if (store.TenantId != tenantId) throw new NotFoundException("Store", request.StoreId);

        var movements = request.ProductId.HasValue
            ? await _uow.StockMovements.GetByProductAsync(tenantId, request.StoreId,
                request.ProductId.Value, request.Limit, ct)
            : await _uow.StockMovements.GetByStoreAsync(tenantId, request.StoreId,
                request.From, request.To, request.Limit, ct);

        var userCache = new Dictionary<Guid, string>();
        var storeCache = new Dictionary<Guid, string>();
        var result = new List<StockMovementResponse>();

        foreach (var m in movements)
        {
            if (!userCache.TryGetValue(m.RecordedByUserId, out var userName))
            {
                var user = await _uow.Users.GetByIdAsync(m.RecordedByUserId, ct);
                userName = user?.FullName ?? m.RecordedByUserId.ToString("N")[..8];
                userCache[m.RecordedByUserId] = userName;
            }

            string? relatedStoreName = null;
            if (m.RelatedStoreId.HasValue)
            {
                if (!storeCache.TryGetValue(m.RelatedStoreId.Value, out relatedStoreName))
                {
                    var rel = await _uow.Stores.GetByIdAsync(m.RelatedStoreId.Value, ct);
                    relatedStoreName = rel?.Name;
                    if (relatedStoreName is not null)
                        storeCache[m.RelatedStoreId.Value] = relatedStoreName;
                }
            }

            result.Add(new StockMovementResponse(
                m.Id,
                m.Type.ToString(),
                m.ProductId,
                m.Product?.Name ?? string.Empty,
                m.Product?.Sku ?? string.Empty,
                m.Quantity,
                m.QuantityBefore,
                m.QuantityAfter,
                m.Reference,
                m.Notes,
                m.RelatedStoreId,
                relatedStoreName,
                m.RecordedByUserId,
                userName,
                m.CreatedAt));
        }

        return result;
    }
}
