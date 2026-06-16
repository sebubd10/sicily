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

        var terminalCount = await _uow.Terminals.CountByStoreAsync(tenantId, store.Id, ct);
        if (terminalCount > 0)
            throw new DomainException(
                $"Cannot delete: {terminalCount} terminal{(terminalCount == 1 ? " is" : "s are")} assigned to this store. Delete or deactivate them first.");

        store.SoftDelete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}
