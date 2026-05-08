using BasicCommerce.Application.Features.ProductReviews;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.ProductReviews;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.ProductReviews.Commands;

public record AddReviewDetailCommand(
    Guid ReviewId,
    string Comment,
    bool IsAdminReply,
    Guid? CommenterUserId = null,
    string? CommenterName = null) : IRequest<ProductReviewResponse>;

public class AddReviewDetailCommandValidator : AbstractValidator<AddReviewDetailCommand>
{
    public AddReviewDetailCommandValidator()
    {
        RuleFor(x => x.ReviewId).NotEmpty();
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(2000);
    }
}

public class AddReviewDetailCommandHandler
    : IRequestHandler<AddReviewDetailCommand, ProductReviewResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public AddReviewDetailCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductReviewResponse> Handle(
        AddReviewDetailCommand request, CancellationToken ct)
    {
        var review = await _uow.ProductReviews.GetWithDetailsAsync(
            _currentUser.TenantId, request.ReviewId, ct)
            ?? throw new NotFoundException("ProductReview", request.ReviewId);

        if (!review.IsApproved && !request.IsAdminReply)
            throw new DomainException("Cannot reply to an unapproved review.");

        review.AddDetail(request.Comment, request.IsAdminReply,
            request.CommenterUserId ?? _currentUser.UserId,
            request.CommenterName);

        _uow.ProductReviews.Update(review);
        await _uow.SaveChangesAsync(ct);

        return ProductReviewMapper.ToResponse(review);
    }
}
