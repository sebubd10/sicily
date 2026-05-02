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

        return new ProductResponse(
            Id: product.Id,
            Sku: product.Sku,
            Barcode: product.Barcode,
            Plu: product.Plu,
            Name: product.Name,
            NameBn: product.NameBn,
            CategoryId: product.CategoryId.ToString(),
            CategoryName: product.Category?.Name ?? string.Empty,
            Price: product.Price.Amount,
            Currency: product.Price.Currency,
            VatRate: vatRate?.Rate ?? 0,
            UnitType: product.UnitType.ToString(),
            IsWeightBased: product.IsWeightBased,
            IsAgeRestricted: product.IsAgeRestricted,
            AgeRestrictionYears: product.AgeRestrictionYears,
            IsActive: product.IsActive,
            ImageUrl: product.ImageUrl);
    }
}
