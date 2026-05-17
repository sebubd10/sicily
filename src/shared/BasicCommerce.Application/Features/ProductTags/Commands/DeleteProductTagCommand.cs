using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.ProductTags.Commands;

public record DeleteProductTagCommand(Guid Id) : IRequest;

public class DeleteProductTagCommandHandler : IRequestHandler<DeleteProductTagCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteProductTagCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteProductTagCommand request, CancellationToken ct)
    {
        var tag = await _uow.ProductTags.GetByIdForTenantAsync(
            _currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("ProductTag", request.Id);

        _uow.ProductTags.Remove(tag);
        await _uow.SaveChangesAsync(ct);
    }
}
