using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

// ── Change a line's quantity on an unfinished sale ─────────────────────────────

public record ChangeLineItemQuantityCommand(
    Guid TenantId, Guid TransactionId, Guid LineItemId, decimal Quantity)
    : IRequest<TransactionResponse>;

public class ChangeLineItemQuantityCommandValidator : AbstractValidator<ChangeLineItemQuantityCommand>
{
    public ChangeLineItemQuantityCommandValidator()
    {
        RuleFor(x => x.LineItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class ChangeLineItemQuantityCommandHandler
    : IRequestHandler<ChangeLineItemQuantityCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ChangeLineItemQuantityCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TransactionResponse> Handle(ChangeLineItemQuantityCommand request, CancellationToken ct)
    {
        var transaction = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.TransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.TransactionId);

        var before = transaction.LineItems.FirstOrDefault(l => l.Id == request.LineItemId)?.Quantity;
        var line = transaction.ChangeLineItemQuantity(request.LineItemId, request.Quantity);

        // The discount was for the old quantity — re-run promotions for the new one
        // (unless the price was manually overridden, which never gets promotions).
        if (!line.IsPriceOverridden)
        {
            var product = await _uow.Products.GetByIdForTenantAsync(request.TenantId, line.ProductId, ct);
            if (product is not null)
            {
                var promotions = await _uow.Promotions.GetActivePromotionsAsync(
                    request.TenantId, transaction.StoreId, ct);
                var (best, discount) = CartPricing.FindBestPromotion(promotions, product, line);
                if (best is not null && discount > 0)
                    transaction.ApplyPromotionToLineItem(line.Id, discount, best.Id, best.Name);
            }
        }

        await TransactionAudit.RecordAsync(_uow, transaction, TransactionAuditAction.ItemQuantityChanged,
            $"Qty of {line.ProductName} changed {before:0.##} → {line.Quantity:0.##}",
            _currentUser.UserId, ct: ct);

        await _uow.SaveChangesAsync(ct);
        return await CreateTransactionCommandHandler.MapToResponseAsync(_uow, transaction, ct);
    }
}

// ── Take a payment back off an unfinished sale ─────────────────────────────────

public record RemovePaymentCommand(Guid TenantId, Guid TransactionId, Guid PaymentId)
    : IRequest<TransactionResponse>;

public class RemovePaymentCommandHandler : IRequestHandler<RemovePaymentCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public RemovePaymentCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TransactionResponse> Handle(RemovePaymentCommand request, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var transaction = await _uow.Transactions.GetWithItemsAsync(
                request.TenantId, request.TransactionId, ct)
                ?? throw new NotFoundException("Transaction", request.TransactionId);

            var payment = transaction.RemovePayment(request.PaymentId);

            // Give back gift card balance / credit / points the payment took.
            await SaleEffects.ReversePaymentAsync(_uow, transaction, payment,
                $"Payment removed from {transaction.TransactionNumber}", ct);

            await TransactionAudit.RecordAsync(_uow, transaction, TransactionAuditAction.PaymentRemoved,
                $"Removed {payment.Method} payment of {TransactionAudit.Money(payment.Amount)}",
                _currentUser.UserId, ct: ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            return await CreateTransactionCommandHandler.MapToResponseAsync(_uow, transaction, ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
