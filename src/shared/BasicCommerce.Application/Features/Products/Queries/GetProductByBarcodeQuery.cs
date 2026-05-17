using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Queries;

public record GetProductByBarcodeQuery(Guid TenantId, string Barcode) : IRequest<ProductResponse>;

public class GetProductByBarcodeQueryValidator : AbstractValidator<GetProductByBarcodeQuery>
{
    public GetProductByBarcodeQueryValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Barcode).NotEmpty().MaximumLength(100);
    }
}

public class GetProductByBarcodeQueryHandler
    : IRequestHandler<GetProductByBarcodeQuery, ProductResponse>
{
    private readonly IUnitOfWork _uow;

    public GetProductByBarcodeQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProductResponse> Handle(GetProductByBarcodeQuery request,
        CancellationToken ct)
    {
        var product = await _uow.Products.GetByBarcodeAsync(request.TenantId, request.Barcode, ct)
            ?? throw new NotFoundException("Product", request.Barcode);

        var vatRate = await _uow.VatRates.GetByIdForTenantAsync(
            request.TenantId, product.VatRateId, ct);

        return ProductMapper.ToResponse(product, vatRate);
    }
}
