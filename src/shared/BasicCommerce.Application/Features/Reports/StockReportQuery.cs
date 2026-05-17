using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Reports;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Reports;

public record StockReportQuery(Guid StoreId) : IRequest<StockReportResponse>;

public class StockReportQueryHandler : IRequestHandler<StockReportQuery, StockReportResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public StockReportQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<StockReportResponse> Handle(StockReportQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);
        if (store.TenantId != tenantId)
            throw new NotFoundException("Store", request.StoreId);

        var stockLevels = await _uow.StockLevels.GetByStoreAsync(tenantId, request.StoreId, ct);

        var categoryCache = new Dictionary<Guid, string>();

        var items = new List<StockItemRow>();
        foreach (var s in stockLevels)
        {
            if (s.Product is null) continue;

            if (!categoryCache.TryGetValue(s.Product.CategoryId, out var categoryName))
            {
                var cat = await _uow.Categories.GetByIdAsync(s.Product.CategoryId, ct);
                categoryName = cat?.Name ?? string.Empty;
                categoryCache[s.Product.CategoryId] = categoryName;
            }

            var available = s.Quantity - s.ReservedQuantity;
            items.Add(new StockItemRow(
                s.ProductId,
                s.Product.Name,
                s.Product.Sku,
                s.Product.Barcode,
                categoryName,
                s.Quantity,
                s.ReservedQuantity,
                available,
                s.LowStockThreshold,
                s.IsLowStock,
                s.Quantity <= 0));
        }

        return new StockReportResponse(
            store.Id,
            store.Name,
            items.Count,
            items.Count(i => i.IsLowStock && !i.IsOutOfStock),
            items.Count(i => i.IsOutOfStock),
            items);
    }
}
