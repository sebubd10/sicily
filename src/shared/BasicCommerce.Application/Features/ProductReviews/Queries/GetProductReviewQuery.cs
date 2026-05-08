using BasicCommerce.Application.Features.ProductReviews;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.ProductReviews;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.ProductReviews.Queries;

public record GetProductReviewQuery(Guid ReviewId) : IRequest<ProductReviewResponse>;

public class GetProductReviewQueryHandler
    : IRequestHandler<GetProductReviewQuery, ProductReviewResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetProductReviewQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductReviewResponse> Handle(
        GetProductReviewQuery request, CancellationToken ct)
    {
        var review = await _uow.ProductReviews.GetWithDetailsAsync(
            _currentUser.TenantId, request.ReviewId, ct)
            ?? throw new NotFoundException("ProductReview", request.ReviewId);

        return ProductReviewMapper.ToResponse(review);
    }
}
