using BasicCommerce.Application.Features.RewardPoints;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record CompleteTransactionCommand(Guid TenantId, Guid TransactionId)
    : IRequest<TransactionResponse>;

public class CompleteTransactionCommandHandler
    : IRequestHandler<CompleteTransactionCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CompleteTransactionCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TransactionResponse> Handle(
        CompleteTransactionCommand request, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var transaction = await _uow.Transactions.GetWithItemsAsync(
                request.TenantId, request.TransactionId, ct)
                ?? throw new NotFoundException("Transaction", request.TransactionId);

            transaction.Complete();

            await SaleEffects.DeductStockAsync(_uow, transaction, ct);
            var points = await RewardPointsLedger.AwardPurchasePointsAsync(_uow, transaction, ct);

            await TransactionAudit.RecordAsync(_uow, transaction, TransactionAuditAction.Completed,
                $"Completed — total {TransactionAudit.Money(transaction.Total)}, paid " +
                $"{TransactionAudit.Money(transaction.AmountPaid)}" +
                (transaction.ChangeDue > 0 ? $", change {TransactionAudit.Money(transaction.ChangeDue)}" : "") +
                (points > 0 ? $", +{points} pts" : ""),
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
