using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.ProductTags.Commands;

public record ActivateProductTagCommand(Guid Id) : IRequest;
public record DeactivateProductTagCommand(Guid Id) : IRequest;

public class ActivateProductTagCommandHandler : IRequestHandler<ActivateProductTagCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ActivateProductTagCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(ActivateProductTagCommand request, CancellationToken ct)
    {
        var tag = await _uow.ProductTags.GetByIdForTenantAsync(_currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("ProductTag", request.Id);

        tag.Activate();
        await _uow.SaveChangesAsync(ct);
    }
}

public class DeactivateProductTagCommandHandler : IRequestHandler<DeactivateProductTagCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeactivateProductTagCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeactivateProductTagCommand request, CancellationToken ct)
    {
        var tag = await _uow.ProductTags.GetByIdForTenantAsync(_currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("ProductTag", request.Id);

        tag.Deactivate();
        await _uow.SaveChangesAsync(ct);
    }
}
