using BasicCommerce.Contracts.Transactions;
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
    public SuspendTransactionCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TransactionResponse> Handle(
        SuspendTransactionCommand request, CancellationToken ct)
    {
        var transaction = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.TransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.TransactionId);

        transaction.Suspend();
        _uow.Transactions.Update(transaction);
        await _uow.SaveChangesAsync(ct);
        return CreateTransactionCommandHandler.MapToResponse(transaction);
    }
}

public class RecallTransactionCommandHandler
    : IRequestHandler<RecallTransactionCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;
    public RecallTransactionCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TransactionResponse> Handle(
        RecallTransactionCommand request, CancellationToken ct)
    {
        var transaction = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.TransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.TransactionId);

        transaction.Recall();
        _uow.Transactions.Update(transaction);
        await _uow.SaveChangesAsync(ct);
        return CreateTransactionCommandHandler.MapToResponse(transaction);
    }
}
