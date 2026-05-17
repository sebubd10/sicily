using BasicCommerce.Application.Features.ProductReviews;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.ProductReviews;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.ProductReviews.Commands;

public record SubmitProductReviewCommand(
    Guid ProductId,
    Guid? CustomerId,
    Guid? StoreId,
    string CustomerName,
    string Title,
    string ReviewText,
    int Rating) : IRequest<ProductReviewResponse>;

public class SubmitProductReviewCommandValidator
    : AbstractValidator<SubmitProductReviewCommand>
{
    public SubmitProductReviewCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ReviewText).NotEmpty().MaximumLength(3000);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
    }
}

public class SubmitProductReviewCommandHandler
    : IRequestHandler<SubmitProductReviewCommand, ProductReviewResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public SubmitProductReviewCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ProductReviewResponse> Handle(
        SubmitProductReviewCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var product = await _uow.Products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        if (product.TenantId != tenantId)
            throw new NotFoundException("Product", request.ProductId);

        var review = ProductReview.Create(
            tenantId, request.ProductId,
            request.CustomerId, request.StoreId,
            request.CustomerName, request.Title, request.ReviewText,
            request.Rating,
            requireApproval: true);

        await _uow.ProductReviews.AddAsync(review, ct);
        await _uow.SaveChangesAsync(ct);

        return ProductReviewMapper.ToResponse(review);
    }
}
