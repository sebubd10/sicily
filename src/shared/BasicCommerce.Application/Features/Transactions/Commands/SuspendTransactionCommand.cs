using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record SuspendTransactionCommand(Guid TenantId, Guid TransactionId)
    : IRequest<TransactionResponse>;

public record RecallTransactionCommand(Guid TenantId, Guid TransactionId)
    : IRequest<TransactionResponse>;

public class SuspendTransactionCommandHandler
    : IRequestHandler<SuspendTransactionCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public SuspendTransactionCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TransactionResponse> Handle(
        SuspendTransactionCommand request, CancellationToken ct)
    {
        var transaction = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.TransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.TransactionId);

        transaction.Suspend();
        await TransactionAudit.RecordAsync(_uow, transaction, TransactionAuditAction.Suspended,
            "Sale put on hold", _currentUser.UserId, ct: ct);

        await _uow.SaveChangesAsync(ct);
        return await CreateTransactionCommandHandler.MapToResponseAsync(_uow, transaction, ct);
    }
}

public class RecallTransactionCommandHandler
    : IRequestHandler<RecallTransactionCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public RecallTransactionCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TransactionResponse> Handle(
        RecallTransactionCommand request, CancellationToken ct)
    {
        var transaction = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.TransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.TransactionId);

        transaction.Recall();
        await TransactionAudit.RecordAsync(_uow, transaction, TransactionAuditAction.Resumed,
            "Sale resumed", _currentUser.UserId, ct: ct);

        await _uow.SaveChangesAsync(ct);
        return await CreateTransactionCommandHandler.MapToResponseAsync(_uow, transaction, ct);
    }
}
