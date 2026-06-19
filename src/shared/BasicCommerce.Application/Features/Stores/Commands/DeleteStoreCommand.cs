using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Stores.Commands;

public record DeleteStoreCommand(Guid StoreId) : IRequest;

public class DeleteStoreCommandHandler : IRequestHandler<DeleteStoreCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteStoreCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteStoreCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);
        if (store.TenantId != tenantId)
            throw new NotFoundException("Store", request.StoreId);

        var reasons = new List<string>();

        var terminalCount = await _uow.Terminals.CountByStoreAsync(tenantId, store.Id, ct);
        if (terminalCount > 0)
            reasons.Add($"{terminalCount} terminal{(terminalCount == 1 ? " is" : "s are")} assigned to this store — delete or deactivate them first");

        if (await _uow.Users.HasUsersAssignedToStoreAsync(tenantId, store.Id, ct))
            reasons.Add("staff members are assigned to this store — reassign them first");

        if (await _uow.TillSessions.HasOpenSessionsForStoreAsync(tenantId, store.Id, ct))
            reasons.Add("there are open till sessions — close them first");

        if (await _uow.StockLevels.HasStockForStoreAsync(tenantId, store.Id, ct))
            reasons.Add("the store has products with stock on hand — transfer or write off inventory first");

        if (await _uow.StockBatches.HasActiveBatchesForStoreAsync(tenantId, store.Id, ct))
            reasons.Add("the store has active stock batches with remaining quantity");

        if (await _uow.SupplierReturns.HasOpenReturnsForStoreAsync(tenantId, store.Id, ct))
            reasons.Add("there are open supplier returns (Draft, Submitted, or Shipped) for this store");

        if (await _uow.GiftCards.HasActiveGiftCardsForStoreAsync(tenantId, store.Id, ct))
            reasons.Add("there are active gift cards with remaining balance issued by this store");

        if (await _uow.CreditAccounts.HasOutstandingCreditForStoreAsync(tenantId, store.Id, ct))
            reasons.Add("customers have outstanding credit balances at this store");

        if (await _uow.RewardPointsAccounts.HasPointsForStoreAsync(tenantId, store.Id, ct))
            reasons.Add("customers have unredeemed reward points tied to this store");

        if (await _uow.Promotions.HasActivePromotionsForStoreAsync(tenantId, store.Id, ct))
            reasons.Add("there are active promotions scoped to this store — deactivate them first");

        if (reasons.Count > 0)
            throw new DomainException(
                $"Cannot delete store '{store.Name}': {string.Join("; ", reasons)}.");

        store.SoftDelete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}
