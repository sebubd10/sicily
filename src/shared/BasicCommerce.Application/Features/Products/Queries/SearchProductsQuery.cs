using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Queries;

public record SearchProductsQuery(Guid TenantId, string Term, int Limit = 20)
    : IRequest<IEnumerable<ProductResponse>>;

public class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
{
    public SearchProductsQueryValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Term).NotEmpty().MinimumLength(2).MaximumLength(100);
        RuleFor(x => x.Limit).InclusiveBetween(1, 100);
    }
}

public class SearchProductsQueryHandler
    : IRequestHandler<SearchProductsQuery, IEnumerable<ProductResponse>>
{
    private readonly IUnitOfWork _uow;

    public SearchProductsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<ProductResponse>> Handle(
        SearchProductsQuery request, CancellationToken ct)
    {
        var products = await _uow.Products.SearchAsync(
            request.TenantId, request.Term, request.Limit, ct);

        return products.Select(p => new ProductResponse(
            p.Id, p.Sku, p.Barcode, p.Plu,
            p.Name, p.NameBn,
            p.CategoryId.ToString(), p.Category?.Name ?? string.Empty,
            p.Price.Amount, p.Price.Currency,
            0, p.UnitType.ToString(),
            p.IsWeightBased, p.IsAgeRestricted,
            p.AgeRestrictionYears, p.IsActive, p.ImageUrl));
    }
}
