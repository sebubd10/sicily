using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Stores.Commands;

public record ActivateStoreCommand(Guid StoreId) : IRequest;

public class ActivateStoreCommandHandler : IRequestHandler<ActivateStoreCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ActivateStoreCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(ActivateStoreCommand request, CancellationToken ct)
    {
        var store = await _uow.Stores.GetByIdAsync(request.StoreId, ct)
            ?? throw new NotFoundException("Store", request.StoreId);
        if (store.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Store", request.StoreId);

        store.Activate();
        await _uow.SaveChangesAsync(ct);
    }
}
