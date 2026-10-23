using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record ReturnItem(
    Guid OriginalLineItemId,
    decimal Quantity,
    ReturnReason Reason,
    DamageDisposition Disposition);

public record CreateReturnTransactionCommand(
    Guid TenantId,
    Guid OriginalTransactionId,
    Guid? TerminalId,
    IEnumerable<ReturnItem> Items,
    PaymentMethod RefundMethod,
    string? Notes = null) : IRequest<TransactionResponse>;

public class CreateReturnTransactionCommandValidator
    : AbstractValidator<CreateReturnTransactionCommand>
{
    public CreateReturnTransactionCommandValidator()
    {
        RuleFor(x => x.OriginalTransactionId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item must be returned.");
        RuleForEach(x => x.Items).ChildRules(item =>
            item.RuleFor(i => i.Quantity).GreaterThan(0));
    }
}

public class CreateReturnTransactionCommandHandler
    : IRequestHandler<CreateReturnTransactionCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateReturnTransactionCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TransactionResponse> Handle(
        CreateReturnTransactionCommand request, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var original = await _uow.Transactions.GetWithItemsAsync(
                request.TenantId, request.OriginalTransactionId, ct)
                ?? throw new NotFoundException("Transaction", request.OriginalTransactionId);

            var returnTx = await ReturnProcessing.BuildReturnAsync(_uow, original,
                request.TerminalId ?? original.TerminalId, request.Items.ToList(), ct);

            var returnTotal = returnTx.Total;
            ReturnProcessing.AddRefund(returnTx, request.RefundMethod, returnTotal,
                $"Refund for {original.TransactionNumber}");
            returnTx.Complete();

            await ReturnProcessing.SettleRewardPointsAsync(_uow, original, returnTx,
                request.RefundMethod == PaymentMethod.RewardPoints ? returnTotal : 0m, ct);

            original.MarkRefunded();

            var itemsText = string.Join(", ", returnTx.LineItems.Select(l => $"{l.Quantity:0.##} × {l.ProductName}"));
            await TransactionAudit.RecordAsync(_uow, original, TransactionAuditAction.Returned,
                $"Returned {itemsText} — refund {TransactionAudit.Money(returnTotal)} by {request.RefundMethod} " +
                $"({returnTx.TransactionNumber})", _currentUser.UserId, request.Notes, ct);
            await TransactionAudit.RecordAsync(_uow, returnTx, TransactionAuditAction.Created,
                $"Return of {itemsText} from {original.TransactionNumber}", _currentUser.UserId, request.Notes, ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            return await CreateTransactionCommandHandler.MapToResponseAsync(_uow, returnTx, ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
