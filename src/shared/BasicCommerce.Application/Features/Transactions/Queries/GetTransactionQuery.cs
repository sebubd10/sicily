using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Queries;

public record GetTransactionQuery(Guid TenantId, Guid TransactionId) : IRequest<TransactionResponse>;

public class GetTransactionQueryHandler : IRequestHandler<GetTransactionQuery, TransactionResponse>
{
    private readonly IUnitOfWork _uow;

    public GetTransactionQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TransactionResponse> Handle(GetTransactionQuery request, CancellationToken ct)
    {
        var transaction = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.TransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.TransactionId);

        string? customerName = null;
        if (transaction.CustomerId.HasValue)
        {
            var customer = await _uow.Customers.GetByIdAsync(transaction.CustomerId.Value, ct);
            customerName = customer?.Name;
        }

        return CreateTransactionCommandHandler.MapToResponse(transaction, customerName);
    }
}
