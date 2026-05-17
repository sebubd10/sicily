using BasicCommerce.Application.Features.Products;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Queries;

public record GetProductQuery(Guid ProductId) : IRequest<ProductResponse>;

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetProductQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductResponse> Handle(GetProductQuery request, CancellationToken ct)
    {
        var product = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        if (product.TenantId != _currentUser.TenantId)
            throw new NotFoundException("Product", request.ProductId);

        var vatRate = await _uow.VatRates.GetByIdAsync(product.VatRateId, ct);
        var category = await _uow.Categories.GetByIdAsync(product.CategoryId, ct);
        var manufacturer = product.ManufacturerId.HasValue
            ? await _uow.Manufacturers.GetByIdAsync(product.ManufacturerId.Value, ct)
            : null;
        return ProductMapper.ToResponse(product, vatRate, category?.Name, manufacturer?.Name);
    }
}
