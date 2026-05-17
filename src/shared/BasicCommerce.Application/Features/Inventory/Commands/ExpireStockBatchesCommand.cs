using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Inventory.Commands;

public record ExpireStockBatchesCommand(Guid StoreId, string? Notes) : IRequest<int>;

public class ExpireStockBatchesCommandHandler : IRequestHandler<ExpireStockBatchesCommand, int>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ExpireStockBatchesCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(ExpireStockBatchesCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);
        if (store.TenantId != tenantId) throw new NotFoundException("Store", request.StoreId);

        var expiredBatches = await _uow.StockBatches.GetExpiredUnprocessedAsync(tenantId, ct);
        var storeBatches = expiredBatches.Where(b => b.StoreId == request.StoreId).ToList();

        if (storeBatches.Count == 0) return 0;

        foreach (var batch in storeBatches)
        {
            var writeOffQty = batch.RemainingQuantity;
            if (writeOffQty <= 0) continue;

            var stockLevel = await _uow.StockLevels.GetAsync(
                tenantId, batch.StoreId, batch.ProductId, ct);

            if (stockLevel is not null && stockLevel.Quantity >= writeOffQty)
            {
                var before = stockLevel.Quantity;
                stockLevel.Decrement(writeOffQty);
                _uow.StockLevels.Update(stockLevel);

                var movement = StockMovement.Create(
                    tenantId, batch.StoreId, batch.ProductId,
                    StockMovementType.ExpiryWriteOff, -writeOffQty, before,
                    _currentUser.UserId,
                    reference: batch.LotNumber,
                    notes: request.Notes ?? $"Expiry write-off: batch {batch.LotNumber ?? batch.Id.ToString()}");
                await _uow.StockMovements.AddAsync(movement, ct);
            }

            batch.MarkExpired();
            _uow.StockBatches.Update(batch);
        }

        await _uow.SaveChangesAsync(ct);
        return storeBatches.Count;
    }
}
