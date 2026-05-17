using BasicCommerce.Application.Features.ProductReviews;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.ProductReviews;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.ProductReviews.Queries;

public record GetProductReviewsQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? ProductId = null,
    Guid? StoreId = null,
    bool? IsApproved = null,
    string? Search = null) : IRequest<ProductReviewListResponse>;

public class GetProductReviewsQueryHandler
    : IRequestHandler<GetProductReviewsQuery, ProductReviewListResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetProductReviewsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductReviewListResponse> Handle(
        GetProductReviewsQuery request, CancellationToken ct)
    {
        var (items, total) = await _uow.ProductReviews.GetPagedAsync(
            _currentUser.TenantId,
            request.Page, request.PageSize,
            request.ProductId, request.StoreId,
            request.IsApproved, request.Search, ct);

        return new ProductReviewListResponse(
            items.Select(ProductReviewMapper.ToSummary),
            total, request.Page, request.PageSize);
    }
}
