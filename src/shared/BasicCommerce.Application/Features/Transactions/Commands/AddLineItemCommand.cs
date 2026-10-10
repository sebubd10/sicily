using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record AddLineItemCommand(
    Guid TenantId,
    Guid TransactionId,
    Guid ProductId,
    decimal Quantity,
    decimal? OverridePrice = null,
    Guid? OverrideApprovedBy = null) : IRequest<TransactionResponse>;

public class AddLineItemCommandValidator : AbstractValidator<AddLineItemCommand>
{
    public AddLineItemCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.OverridePrice).GreaterThan(0)
            .When(x => x.OverridePrice.HasValue);
        RuleFor(x => x.OverrideApprovedBy).NotEmpty()
            .When(x => x.OverridePrice.HasValue)
            .WithMessage("Supervisor approval is required for price overrides.");
    }
}

public class AddLineItemCommandHandler : IRequestHandler<AddLineItemCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;

    public AddLineItemCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TransactionResponse> Handle(AddLineItemCommand request, CancellationToken ct)
    {
        var transaction = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.TransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.TransactionId);

        var product = await _uow.Products.GetByIdForTenantAsync(
            request.TenantId, request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        var taxRate = await CartPricing.GetVatRateAsync(_uow, request.TenantId, product, ct);

        var lineItem = transaction.AddItem(product, request.Quantity,
            request.OverridePrice, request.OverrideApprovedBy, taxRate);

        // Auto-apply best matching promotion (non-coupon only)
        if (request.OverridePrice is null)
        {
            var promotions = await _uow.Promotions.GetActivePromotionsAsync(
                request.TenantId, transaction.StoreId, ct);
            var (best, discount) = CartPricing.FindBestPromotion(promotions, product, lineItem);

            if (best is not null && discount > 0)
            {
                transaction.ApplyPromotionToLineItem(lineItem.Id, discount, best.Id, best.Name);
                best.RecordUse();
            }
        }

        await _uow.SaveChangesAsync(ct);

        return CreateTransactionCommandHandler.MapToResponse(transaction);
    }
}
