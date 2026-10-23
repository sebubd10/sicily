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

public record VoidTransactionCommand(
    Guid TenantId,
    Guid TransactionId,
    Guid SupervisorId,
    string Reason) : IRequest<TransactionResponse>;

public class VoidTransactionCommandValidator : AbstractValidator<VoidTransactionCommand>
{
    public VoidTransactionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.SupervisorId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class VoidTransactionCommandHandler
    : IRequestHandler<VoidTransactionCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;

    public VoidTransactionCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TransactionResponse> Handle(
        VoidTransactionCommand request, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var transaction = await _uow.Transactions.GetWithItemsAsync(
                request.TenantId, request.TransactionId, ct)
                ?? throw new NotFoundException("Transaction", request.TransactionId);

            await VoidCoreAsync(_uow, transaction, request.SupervisorId, request.Reason,
                "Voided", ct);

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

    /// <summary>
    /// Voids a sale and undoes everything it did: payments taken (gift card balance, credit
    /// charge, points redeemed), and — if it was completed — the stock it took and the points
    /// it earned. Caller owns SaveChanges and the database transaction.
    /// </summary>
    internal static async Task VoidCoreAsync(IUnitOfWork uow, Transaction transaction,
        Guid supervisorId, string reason, string auditSummary, CancellationToken ct)
    {
        var supervisor = await uow.Users.GetByIdForTenantAsync(transaction.TenantId, supervisorId, ct)
            ?? throw new NotFoundException("Supervisor", supervisorId);
        if ((int)supervisor.Role < (int)UserRole.Supervisor)
            throw new UnauthorizedException("Only a supervisor or above can void transactions.");

        var wasCompleted = transaction.TransactionStatus == TransactionStatus.Completed;
        if (wasCompleted)
            await TillRules.EnsureSaleInOpenTillSessionAsync(uow, transaction, "voided", ct);

        transaction.Void(supervisorId, reason);

        var note = $"Void {transaction.TransactionNumber}";
        foreach (var payment in transaction.Payments.Where(p => p.PaymentStatus == PaymentStatus.Approved).ToList())
        {
            await SaleEffects.ReversePaymentAsync(uow, transaction, payment, note, ct);
            payment.MarkRefunded();
        }

        if (wasCompleted)
        {
            await SaleEffects.RestoreStockAsync(uow, transaction, supervisorId, ct);
            await ReverseEarnedPointsAsync(uow, transaction, note, ct);
        }

        await TransactionAudit.RecordAsync(uow, transaction, TransactionAuditAction.Voided,
            auditSummary, supervisorId, reason, ct);
    }

    private static async Task ReverseEarnedPointsAsync(IUnitOfWork uow, Transaction transaction,
        string note, CancellationToken ct)
    {
        if (!transaction.CustomerId.HasValue) return;

        var settings = await uow.RewardPointsSettings.GetByTenantAsync(transaction.TenantId, ct);
        if (settings is null) return;

        var account = await RewardPointsLedger.GetAccountAsync(uow, settings, transaction.TenantId,
            transaction.CustomerId.Value, transaction.StoreId, ct);
        if (account is null) return;

        var entries = await uow.RewardPointsAccounts.GetEntriesForTransactionAsync(transaction.Id, ct);
        var reversed = account.ReversePoints(RewardPointsLedger.NetEarned(entries), transaction.Id, note);
        await RewardPointsLedger.AdjustCustomerSummaryAsync(uow, transaction.CustomerId.Value, -reversed, ct);
    }
}

// ── Void and re-ring ───────────────────────────────────────────────────────────

/// <summary>
/// Voids a sale and opens a new draft with the same customer and items, so the cashier can
/// fix it and take payment again. Prices and promotions are re-evaluated at today's values.
/// </summary>
public record VoidAndReRingCommand(
    Guid TenantId, Guid TransactionId, Guid SupervisorId, string Reason)
    : IRequest<TransactionResponse>;

public class VoidAndReRingCommandValidator : AbstractValidator<VoidAndReRingCommand>
{
    public VoidAndReRingCommandValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class VoidAndReRingCommandHandler : IRequestHandler<VoidAndReRingCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public VoidAndReRingCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TransactionResponse> Handle(VoidAndReRingCommand request, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var original = await _uow.Transactions.GetWithItemsAsync(
                request.TenantId, request.TransactionId, ct)
                ?? throw new NotFoundException("Transaction", request.TransactionId);

            var draft = Transaction.Create(original.TenantId, original.StoreId, original.TerminalId,
                _currentUser.UserId, original.CustomerId);
            await _uow.Transactions.AddAsync(draft, ct);

            await VoidTransactionCommandHandler.VoidCoreAsync(_uow, original, request.SupervisorId,
                request.Reason, $"Voided and re-rung as {draft.TransactionNumber}", ct);

            var promotions = (await _uow.Promotions.GetActivePromotionsAsync(
                original.TenantId, original.StoreId, ct)).ToList();

            foreach (var line in original.LineItems.Where(l => !l.IsVoided))
            {
                var product = await _uow.Products.GetByIdForTenantAsync(original.TenantId, line.ProductId, ct);
                if (product is null) continue;   // product deleted since — cashier can add a replacement

                var taxRate = await CartPricing.GetVatRateAsync(_uow, original.TenantId, product, ct);
                var newLine = line.IsPriceOverridden
                    ? draft.AddItem(product, line.Quantity, line.UnitPrice, line.PriceOverrideApprovedBy, taxRate)
                    : draft.AddItem(product, line.Quantity, taxRate: taxRate);

                if (!newLine.IsPriceOverridden)
                {
                    var (best, discount) = CartPricing.FindBestPromotion(promotions, product, newLine);
                    if (best is not null && discount > 0)
                        draft.ApplyPromotionToLineItem(newLine.Id, discount, best.Id, best.Name);
                }
            }

            await TransactionAudit.RecordAsync(_uow, draft, TransactionAuditAction.ReRung,
                $"Re-rung from voided sale {original.TransactionNumber}",
                _currentUser.UserId, request.Reason, ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            return await CreateTransactionCommandHandler.MapToResponseAsync(_uow, draft, ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
