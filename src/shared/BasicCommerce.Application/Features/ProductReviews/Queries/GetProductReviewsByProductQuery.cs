using BasicCommerce.Application.Features.ProductReviews;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.ProductReviews;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.ProductReviews.Queries;

public record GetProductReviewsByProductQuery(
    Guid ProductId,
    int Page = 1,
    int PageSize = 10,
    bool ApprovedOnly = true) : IRequest<ProductReviewListResponse>;

public class GetProductReviewsByProductQueryHandler
    : IRequestHandler<GetProductReviewsByProductQuery, ProductReviewListResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetProductReviewsByProductQueryHandler(
        IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductReviewListResponse> Handle(
        GetProductReviewsByProductQuery request, CancellationToken ct)
    {
        var (items, total) = await _uow.ProductReviews.GetByProductAsync(
            _currentUser.TenantId, request.ProductId,
            request.Page, request.PageSize, request.ApprovedOnly, ct);

        return new ProductReviewListResponse(
            items.Select(ProductReviewMapper.ToSummary),
            total, request.Page, request.PageSize);
    }
}

public record GetProductRatingSummaryQuery(Guid ProductId)
    : IRequest<ProductRatingSummaryResponse>;

public class GetProductRatingSummaryQueryHandler
    : IRequestHandler<GetProductRatingSummaryQuery, ProductRatingSummaryResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetProductRatingSummaryQueryHandler(
        IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductRatingSummaryResponse> Handle(
        GetProductRatingSummaryQuery request, CancellationToken ct)
    {
        var (avg, count) = await _uow.ProductReviews.GetProductRatingSummaryAsync(
            _currentUser.TenantId, request.ProductId, ct);

        return new ProductRatingSummaryResponse(request.ProductId, avg, count);
    }
}
