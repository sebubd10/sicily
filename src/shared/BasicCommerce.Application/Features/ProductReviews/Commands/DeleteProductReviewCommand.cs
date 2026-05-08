using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.ProductReviews.Commands;

public record DeleteProductReviewCommand(Guid ReviewId) : IRequest;

public class DeleteProductReviewCommandHandler : IRequestHandler<DeleteProductReviewCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteProductReviewCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteProductReviewCommand request, CancellationToken ct)
    {
        var review = await _uow.ProductReviews.GetByIdForTenantAsync(
            _currentUser.TenantId, request.ReviewId, ct)
            ?? throw new NotFoundException("ProductReview", request.ReviewId);

        review.Delete();
        _uow.ProductReviews.Update(review);
        await _uow.SaveChangesAsync(ct);
    }
}

public record DeleteReviewDetailCommand(Guid ReviewId, Guid DetailId) : IRequest;

public class DeleteReviewDetailCommandHandler : IRequestHandler<DeleteReviewDetailCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteReviewDetailCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteReviewDetailCommand request, CancellationToken ct)
    {
        var review = await _uow.ProductReviews.GetWithDetailsAsync(
            _currentUser.TenantId, request.ReviewId, ct)
            ?? throw new NotFoundException("ProductReview", request.ReviewId);

        var detail = review.Details.FirstOrDefault(d => d.Id == request.DetailId)
            ?? throw new NotFoundException("ProductReviewDetail", request.DetailId);

        detail.Delete();
        _uow.ProductReviews.Update(review);
        await _uow.SaveChangesAsync(ct);
    }
}
