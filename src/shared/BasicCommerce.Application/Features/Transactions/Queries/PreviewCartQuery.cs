using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Queries;

/// <summary>
/// Prices a cart without saving anything. Builds a throwaway in-memory transaction and
/// runs the same domain + promotion logic as a real sale, so the preview total always
/// equals what CompleteTransaction will charge.
/// </summary>
public record PreviewCartQuery(
    Guid TenantId,
    Guid StoreId,
    IReadOnlyList<CartPreviewItemRequest> Items) : IRequest<CartPreviewResponse>;

public class PreviewCartQueryValidator : AbstractValidator<PreviewCartQuery>
{
    public PreviewCartQueryValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(i =>
        {
            i.RuleFor(x => x.ProductId).NotEmpty();
            i.RuleFor(x => x.Quantity).GreaterThan(0);
        });
    }
}

public class PreviewCartQueryHandler : IRequestHandler<PreviewCartQuery, CartPreviewResponse>
{
    private readonly IUnitOfWork _uow;

    public PreviewCartQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<CartPreviewResponse> Handle(PreviewCartQuery request, CancellationToken ct)
    {
        var draft = Domain.Entities.Transaction.Create(
            request.TenantId, request.StoreId, Guid.Empty, Guid.Empty);

        var promotions = (await _uow.Promotions.GetActivePromotionsAsync(
            request.TenantId, request.StoreId, ct)).ToList();

        foreach (var item in request.Items)
        {
            var product = await _uow.Products.GetByIdForTenantAsync(
                request.TenantId, item.ProductId, ct)
                ?? throw new NotFoundException("Product", item.ProductId);

            var taxRate = await CartPricing.GetVatRateAsync(_uow, request.TenantId, product, ct);
            var line = draft.AddItem(product, item.Quantity, taxRate: taxRate);

            var (best, discount) = CartPricing.FindBestPromotion(promotions, product, line);
            if (best is not null && discount > 0)
                draft.ApplyPromotionToLineItem(line.Id, discount, best.Id, best.Name);
        }

        var lines = draft.LineItems.Select(l => new CartPreviewLine(
            l.ProductId, l.ProductName, l.ProductSku, l.Quantity, l.UnitPrice,
            LineSubTotal: l.LineTotal,
            DiscountAmount: l.DiscountAmount,
            TaxRate: l.TaxRate,
            TaxAmount: l.TaxAmount,
            LineTotal: l.LineTotal - l.DiscountAmount + l.TaxAmount,
            PromotionId: l.AppliedPromotionId,
            PromotionName: l.AppliedPromotionName)).ToList();

        return new CartPreviewResponse(
            lines, draft.SubTotal, draft.DiscountTotal, draft.TaxTotal, draft.Total);
    }
}
