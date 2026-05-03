using BasicCommerce.Application.Features.Products;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Queries;

public record GetProductsQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? CategoryId = null,
    EntityStatus? Status = null) : IRequest<ProductListResponse>;

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
        var (items, total) = await _uow.Products.GetPagedAsync(
            tenantId, request.Page, request.PageSize, request.CategoryId, request.Status, ct);

        var vatRateIds = items.Select(p => p.VatRateId).Distinct().ToList();
        var vatRates = new Dictionary<Guid, VatRate>();
        foreach (var id in vatRateIds)
        {
            var vr = await _uow.VatRates.GetByIdAsync(id, ct);
            if (vr is not null) vatRates[id] = vr;
        }

        var responses = items.Select(p =>
            ProductMapper.ToResponse(p, vatRates.GetValueOrDefault(p.VatRateId), p.Category?.Name));

        return new ProductListResponse(responses, total, request.Page, request.PageSize);
    }
}
