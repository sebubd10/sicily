using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Queries;

public record GetProductByPluQuery(Guid TenantId, string Plu) : IRequest<ProductResponse>;

public class GetProductByPluQueryValidator : AbstractValidator<GetProductByPluQuery>
{
    public GetProductByPluQueryValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Plu).NotEmpty().MaximumLength(20);
    }
}

public class GetProductByPluQueryHandler : IRequestHandler<GetProductByPluQuery, ProductResponse>
{
    private readonly IUnitOfWork _uow;

    public GetProductByPluQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProductResponse> Handle(GetProductByPluQuery request, CancellationToken ct)
    {
        var product = await _uow.Products.GetByPluAsync(request.TenantId, request.Plu, ct)
            ?? throw new NotFoundException("Product", request.Plu);

        var vatRate = await _uow.VatRates.GetByIdForTenantAsync(
            request.TenantId, product.VatRateId, ct);

        return new ProductResponse(
            product.Id, product.Sku, product.Barcode, product.Plu,
            product.Name, product.NameBn,
            product.CategoryId.ToString(), product.Category?.Name ?? string.Empty,
            product.Price.Amount, product.Price.Currency,
            vatRate?.Rate ?? 0, product.UnitType.ToString(),
            product.IsWeightBased, product.IsAgeRestricted,
            product.AgeRestrictionYears, product.Status.ToString(), product.ImageUrl);
    }
}
