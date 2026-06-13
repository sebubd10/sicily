using BasicCommerce.Application.Features.Products;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.ProductTags.Commands;

public record SetProductTagsCommand(
    Guid ProductId,
    IEnumerable<Guid> TagIds) : IRequest<ProductResponse>;

public class SetProductTagsCommandHandler
    : IRequestHandler<SetProductTagsCommand, ProductResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public SetProductTagsCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductResponse> Handle(SetProductTagsCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var product = await _uow.Products.GetWithTagsAsync(tenantId, request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        var existingTagIds = product.Tags.Select(t => t.Id);
        var tags = await _uow.ProductTags.GetByIdsAsync(tenantId, request.TagIds, ct);
        ProductTagAssignmentValidator.EnsureNoNewInactiveTags(tags, existingTagIds);
        product.SetTags(tags);

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(ct);

        var vatRate = await _uow.VatRates.GetByIdAsync(product.VatRateId, ct);
        var category = await _uow.Categories.GetByIdAsync(product.CategoryId, ct);
        return ProductMapper.ToResponse(product, vatRate, category?.Name);
    }
}
