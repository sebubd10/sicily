using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Queries;

public record GetProductsQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? CategoryId = null,
    EntityStatus? Status = null,
    string? Search = null) : IRequest<ProductListResponse>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, ProductListResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetProductsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductListResponse> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var (items, total) = await _uow.Products.GetPagedProjectedAsync(
            tenantId, request.Page, request.PageSize, request.CategoryId, request.Status,
            request.Search, ct);

        var responses = items.Select(i => new ProductListItemResponse(
            i.Id, i.Sku, i.Name, i.NameBn,
            i.CategoryName, i.Price, i.Currency,
            i.VatRate, i.ImageUrl, i.ManufacturerName, i.Status));

        return new ProductListResponse(responses, total, request.Page, request.PageSize);
    }
}
