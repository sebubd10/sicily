using BasicCommerce.Application.Features.ProductTags.Queries;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.ProductTags;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.ProductTags.Commands;

public record CreateProductTagCommand(string Name) : IRequest<ProductTagDetailResponse>;

public class CreateProductTagCommandValidator : AbstractValidator<CreateProductTagCommand>
{
    public CreateProductTagCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class CreateProductTagCommandHandler
    : IRequestHandler<CreateProductTagCommand, ProductTagDetailResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateProductTagCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductTagDetailResponse> Handle(
        CreateProductTagCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var trimmed = request.Name.Trim();

        if (await _uow.ProductTags.GetByNameAsync(tenantId, trimmed, ct) is not null)
            throw new DomainException($"Tag '{trimmed}' already exists.");

        var tag = ProductTag.Create(tenantId, trimmed);
        await _uow.ProductTags.AddAsync(tag, ct);
        await _uow.SaveChangesAsync(ct);

        return GetProductTagsQueryHandler.MapToResponse(tag, 0);
    }
}
