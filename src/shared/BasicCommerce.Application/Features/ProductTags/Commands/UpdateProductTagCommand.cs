using BasicCommerce.Application.Features.ProductTags.Queries;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.ProductTags;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.ProductTags.Commands;

public record UpdateProductTagCommand(Guid Id, string Name) : IRequest<ProductTagDetailResponse>;

public class UpdateProductTagCommandValidator : AbstractValidator<UpdateProductTagCommand>
{
    public UpdateProductTagCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateProductTagCommandHandler
    : IRequestHandler<UpdateProductTagCommand, ProductTagDetailResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateProductTagCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductTagDetailResponse> Handle(
        UpdateProductTagCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var trimmed = request.Name.Trim();

        var tag = await _uow.ProductTags.GetByIdForTenantAsync(tenantId, request.Id, ct)
            ?? throw new NotFoundException("ProductTag", request.Id);

        var conflict = await _uow.ProductTags.GetByNameAsync(tenantId, trimmed, ct);
        if (conflict is not null && conflict.Id != tag.Id)
            throw new DomainException($"Tag '{trimmed}' already exists.");

        tag.Rename(trimmed);
        _uow.ProductTags.Update(tag);
        await _uow.SaveChangesAsync(ct);

        return GetProductTagsQueryHandler.MapToResponse(tag, tag.Products.Count);
    }
}
