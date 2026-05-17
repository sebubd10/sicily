using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Promotions;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Promotions.Commands;

public record CreatePromotionCommand(
    string Name,
    string? Description,
    PromotionType Type,
    Guid? ProductId,
    Guid? CategoryId,
    Guid? StoreId,
    decimal? DiscountPercentage,
    decimal? DiscountAmount,
    int? BuyQuantity,
    int? GetQuantity,
    decimal? MinimumCartValue,
    string? CouponCode,
    bool RequiresCoupon,
    DateTime? StartsAt,
    DateTime? EndsAt,
    int? MaxUses) : IRequest<PromotionResponse>;

public class CreatePromotionCommandValidator : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MaxUses).GreaterThan(0).When(x => x.MaxUses.HasValue);
        RuleFor(x => x.EndsAt).GreaterThan(x => x.StartsAt)
            .When(x => x.EndsAt.HasValue && x.StartsAt.HasValue)
            .WithMessage("EndsAt must be after StartsAt.");
        RuleFor(x => x.CouponCode).MaximumLength(50).When(x => x.CouponCode != null);
    }
}

public class CreatePromotionCommandHandler
    : IRequestHandler<CreatePromotionCommand, PromotionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreatePromotionCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<PromotionResponse> Handle(
        CreatePromotionCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var existing = await _uow.Promotions.GetByCouponCodeAsync(
                tenantId, request.CouponCode, ct);
            if (existing is not null)
                throw new DomainException($"Coupon code '{request.CouponCode}' already exists.");
        }

        var promotion = Promotion.Create(tenantId, request.Name, request.Description,
            request.Type, request.ProductId, request.CategoryId, request.StoreId,
            request.DiscountPercentage, request.DiscountAmount,
            request.BuyQuantity, request.GetQuantity,
            request.MinimumCartValue, request.CouponCode, request.RequiresCoupon,
            request.StartsAt, request.EndsAt, request.MaxUses);

        await _uow.Promotions.AddAsync(promotion, ct);
        await _uow.SaveChangesAsync(ct);
        return PromotionMapper.ToResponse(promotion);
    }
}
