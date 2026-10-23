using BasicCommerce.Application.Features.RewardPoints;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record ExchangeNewItem(Guid ProductId, decimal Quantity);

/// <summary>
/// Returns items from a completed sale and sells replacements in one step. The value of the
/// returned items is carried to the replacement sale as "exchange credit"; only the
/// difference is paid or refunded with real money.
/// </summary>
public record ExchangeTransactionCommand(
    Guid TenantId,
    Guid OriginalTransactionId,
    IReadOnlyList<ReturnItem> ReturnItems,
    IReadOnlyList<ExchangeNewItem> NewItems,
    PaymentMethod SettlementMethod,
    string? SettlementMobileNumber,
    string? Notes) : IRequest<ExchangeResponse>;

public class ExchangeTransactionCommandValidator : AbstractValidator<ExchangeTransactionCommand>
{
    public ExchangeTransactionCommandValidator()
    {
        RuleFor(x => x.OriginalTransactionId).NotEmpty();
        RuleFor(x => x.ReturnItems).NotEmpty().WithMessage("Pick at least one item to return.");
        RuleFor(x => x.NewItems).NotEmpty().WithMessage("Add at least one replacement item.");
        RuleForEach(x => x.ReturnItems).ChildRules(i => i.RuleFor(r => r.Quantity).GreaterThan(0));
        RuleForEach(x => x.NewItems).ChildRules(i => i.RuleFor(n => n.Quantity).GreaterThan(0));
        RuleFor(x => x.SettlementMethod)
            .Must(Transaction.IsSimpleTender)
            .WithMessage("The difference can be settled by Cash, Card, bKash, Nagad or Rocket.");
        RuleFor(x => x.SettlementMobileNumber).NotEmpty()
            .When(x => x.SettlementMethod is PaymentMethod.BKash or PaymentMethod.Nagad or PaymentMethod.Rocket)
            .WithMessage("Mobile number is required for mobile wallet payments.");
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public class ExchangeTransactionCommandHandler
    : IRequestHandler<ExchangeTransactionCommand, ExchangeResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ExchangeTransactionCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ExchangeResponse> Handle(ExchangeTransactionCommand request, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var original = await _uow.Transactions.GetWithItemsAsync(
                request.TenantId, request.OriginalTransactionId, ct)
                ?? throw new NotFoundException("Transaction", request.OriginalTransactionId);

            var userId = _currentUser.UserId;

            // 1. Replacement sale — priced first so we know how to split the refund.
            var replacement = Transaction.CreateExchange(original.TenantId, original.StoreId,
                original.TerminalId, userId, original.Id, original.CustomerId);
            await _uow.Transactions.AddAsync(replacement, ct);

            var promotions = (await _uow.Promotions.GetActivePromotionsAsync(
                original.TenantId, original.StoreId, ct)).ToList();

            foreach (var item in request.NewItems)
            {
                var product = await _uow.Products.GetByIdForTenantAsync(original.TenantId, item.ProductId, ct)
                    ?? throw new NotFoundException("Product", item.ProductId);

                var taxRate = await CartPricing.GetVatRateAsync(_uow, original.TenantId, product, ct);
                var line = replacement.AddItem(product, item.Quantity, taxRate: taxRate);

                var (best, discount) = CartPricing.FindBestPromotion(promotions, product, line);
                if (best is not null && discount > 0)
                {
                    replacement.ApplyPromotionToLineItem(line.Id, discount, best.Id, best.Name);
                    best.RecordUse();
                }
            }

            // 2. Return — restocks, reverses points on the returned share.
            var returnTx = await ReturnProcessing.BuildReturnAsync(_uow, original,
                original.TerminalId, request.ReturnItems, ct);

            var returnValue = returnTx.Total;
            var newTotal = replacement.Total;
            var carried = Math.Min(returnValue, newTotal);
            var difference = newTotal - returnValue;   // > 0 customer pays, < 0 customer refunded

            // Return refund: exchange credit for what carries over, money for any surplus.
            ReturnProcessing.AddRefund(returnTx, PaymentMethod.ExchangeCredit, carried,
                $"Exchange credit to {replacement.TransactionNumber}");
            if (difference < 0)
            {
                var refund = ReturnProcessing.AddRefund(returnTx, request.SettlementMethod, -difference,
                    $"Exchange refund for {original.TransactionNumber}");
                if (refund is not null && request.SettlementMobileNumber is not null)
                    refund.SetMobileNumber(request.SettlementMobileNumber);
            }
            returnTx.Complete();

            await ReturnProcessing.SettleRewardPointsAsync(_uow, original, returnTx, 0m, ct);
            original.MarkRefunded();

            // 3. Pay for the replacement: exchange credit first, then the difference.
            if (carried > 0)
            {
                var credit = replacement.AddPayment(PaymentMethod.ExchangeCredit, carried,
                    $"Exchange credit from {returnTx.TransactionNumber}");
                credit.Approve();
                replacement.RefreshAmountPaid();
            }
            if (difference > 0)
            {
                var topUp = replacement.AddPayment(request.SettlementMethod, difference,
                    $"Exchange difference");
                if (request.SettlementMobileNumber is not null)
                    topUp.SetMobileNumber(request.SettlementMobileNumber);
                topUp.Approve();
                replacement.RefreshAmountPaid();
            }

            replacement.Complete();
            await SaleEffects.DeductStockAsync(_uow, replacement, ct);
            var points = await RewardPointsLedger.AwardPurchasePointsAsync(_uow, replacement, ct);

            // 4. Audit all three.
            var returnedText = string.Join(", ", returnTx.LineItems.Select(l => $"{l.Quantity:0.##} × {l.ProductName}"));
            var newText = string.Join(", ", replacement.LineItems.Select(l => $"{l.Quantity:0.##} × {l.ProductName}"));
            var settleText = difference switch
            {
                > 0 => $"customer paid {TransactionAudit.Money(difference)} by {request.SettlementMethod}",
                < 0 => $"customer refunded {TransactionAudit.Money(-difference)} by {request.SettlementMethod}",
                _   => "no difference",
            };

            await TransactionAudit.RecordAsync(_uow, original, TransactionAuditAction.Exchanged,
                $"Exchanged {returnedText} for {newText} — {settleText} " +
                $"({returnTx.TransactionNumber} / {replacement.TransactionNumber})",
                userId, request.Notes, ct);
            await TransactionAudit.RecordAsync(_uow, returnTx, TransactionAuditAction.Created,
                $"Exchange return of {returnedText} from {original.TransactionNumber}", userId, request.Notes, ct);
            await TransactionAudit.RecordAsync(_uow, replacement, TransactionAuditAction.Created,
                $"Exchange for {original.TransactionNumber}: {newText} — {settleText}" +
                (points > 0 ? $", +{points} pts" : ""), userId, request.Notes, ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            return new ExchangeResponse(
                await CreateTransactionCommandHandler.MapToResponseAsync(_uow, returnTx, ct),
                await CreateTransactionCommandHandler.MapToResponseAsync(_uow, replacement, ct),
                difference);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
