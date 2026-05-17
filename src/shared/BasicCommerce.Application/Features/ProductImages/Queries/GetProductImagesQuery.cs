using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.ProductImages.Queries;

public record GetProductImagesQuery(Guid ProductId) : IRequest<IEnumerable<ProductImageResponse>>;

public class GetProductImagesQueryHandler : IRequestHandler<GetProductImagesQuery, IEnumerable<ProductImageResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetProductImagesQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<ProductImageResponse>> Handle(GetProductImagesQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var product = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        if (product.TenantId != tenantId)
            throw new NotFoundException("Product", request.ProductId);

        var images = await _uow.ProductImages.GetByProductAsync(tenantId, request.ProductId, ct);
        return images.Select(ToResponse);
    }

    private static ProductImageResponse ToResponse(Domain.Entities.ProductImage img) =>
        new(img.Id, img.ProductId, img.Title, img.Description, img.Url,
            img.IsUploaded, img.SortOrder, img.CreatedAt, img.UpdatedAt);
}
