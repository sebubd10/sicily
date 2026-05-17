using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.ProductTags;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.ProductTags.Queries;

public record GetProductTagsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IRequest<ProductTagListResponse>;

public class GetProductTagsQueryHandler
    : IRequestHandler<GetProductTagsQuery, ProductTagListResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetProductTagsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductTagListResponse> Handle(
        GetProductTagsQuery request, CancellationToken ct)
    {
        var (items, total) = await _uow.ProductTags.GetPagedAsync(
            _currentUser.TenantId, request.Page, request.PageSize, request.Search, ct);

        return new ProductTagListResponse(
            items.Select(x => MapToResponse(x.Tag, x.TaggedProductsCount)),
            total,
            request.Page,
            request.PageSize);
    }

    internal static ProductTagDetailResponse MapToResponse(
        Domain.Entities.ProductTag t, int count) => new(
        t.Id, t.Name, count, t.Status.ToString(), t.CreatedAt, t.UpdatedAt);
}
