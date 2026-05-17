using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.ProductTags.Commands;

public record BulkDeleteProductTagsCommand(IEnumerable<Guid> Ids) : IRequest;

public class BulkDeleteProductTagsCommandValidator
    : AbstractValidator<BulkDeleteProductTagsCommand>
{
    public BulkDeleteProductTagsCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty().WithMessage("At least one tag ID is required.");
    }
}

public class BulkDeleteProductTagsCommandHandler : IRequestHandler<BulkDeleteProductTagsCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public BulkDeleteProductTagsCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(BulkDeleteProductTagsCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var tags = await _uow.ProductTags.GetByIdsAsync(tenantId, request.Ids, ct);

        foreach (var tag in tags)
            _uow.ProductTags.Remove(tag);

        await _uow.SaveChangesAsync(ct);
    }
}
