using BasicCommerce.Application.Features.ProductReviews;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.ProductReviews;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.ProductReviews.Commands;

public record ApproveProductReviewCommand(Guid ReviewId) : IRequest<ProductReviewResponse>;

public class ApproveProductReviewCommandHandler
    : IRequestHandler<ApproveProductReviewCommand, ProductReviewResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ApproveProductReviewCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductReviewResponse> Handle(
        ApproveProductReviewCommand request, CancellationToken ct)
    {
        var review = await _uow.ProductReviews.GetWithDetailsAsync(
            _currentUser.TenantId, request.ReviewId, ct)
            ?? throw new NotFoundException("ProductReview", request.ReviewId);

        review.Approve();
        _uow.ProductReviews.Update(review);
        await _uow.SaveChangesAsync(ct);

        return ProductReviewMapper.ToResponse(review);
    }
}

public record RejectProductReviewCommand(Guid ReviewId) : IRequest<ProductReviewResponse>;

public class RejectProductReviewCommandHandler
    : IRequestHandler<RejectProductReviewCommand, ProductReviewResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public RejectProductReviewCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductReviewResponse> Handle(
        RejectProductReviewCommand request, CancellationToken ct)
    {
        var review = await _uow.ProductReviews.GetWithDetailsAsync(
            _currentUser.TenantId, request.ReviewId, ct)
            ?? throw new NotFoundException("ProductReview", request.ReviewId);

        review.Reject();
        _uow.ProductReviews.Update(review);
        await _uow.SaveChangesAsync(ct);

        return ProductReviewMapper.ToResponse(review);
    }
}
