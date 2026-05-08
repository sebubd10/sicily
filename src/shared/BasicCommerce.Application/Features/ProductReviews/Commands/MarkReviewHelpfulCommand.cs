using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.ProductReviews.Commands;

public record MarkReviewHelpfulCommand(
    Guid ReviewId,
    Guid CustomerId,
    bool IsHelpful) : IRequest;

public class MarkReviewHelpfulCommandHandler : IRequestHandler<MarkReviewHelpfulCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public MarkReviewHelpfulCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkReviewHelpfulCommand request, CancellationToken ct)
    {
        var review = await _uow.ProductReviews.GetByIdForTenantAsync(
            _currentUser.TenantId, request.ReviewId, ct)
            ?? throw new NotFoundException("ProductReview", request.ReviewId);

        if (!review.IsApproved)
            throw new DomainException("Cannot vote on an unapproved review.");

        var existing = await _uow.ProductReviews.GetHelpfulnessVoteAsync(
            request.ReviewId, request.CustomerId, ct);

        if (existing is null)
        {
            var vote = ProductReviewHelpfulness.Create(
                request.ReviewId, request.CustomerId, request.IsHelpful);
            await _uow.ProductReviews.AddHelpfulnessVoteAsync(vote, ct);
            review.IncrementHelpfulVote(request.IsHelpful);
        }
        else if (existing.IsHelpful != request.IsHelpful)
        {
            review.ChangeHelpfulVote(existing.IsHelpful, request.IsHelpful);
            existing.ChangeVote(request.IsHelpful);
            _uow.ProductReviews.UpdateHelpfulnessVote(existing);
        }

        _uow.ProductReviews.Update(review);
        await _uow.SaveChangesAsync(ct);
    }
}
