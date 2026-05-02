using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record AttachCustomerCommand(Guid TenantId, Guid TransactionId, Guid CustomerId)
    : IRequest<TransactionResponse>;

public class AttachCustomerCommandHandler : IRequestHandler<AttachCustomerCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;
    public AttachCustomerCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TransactionResponse> Handle(
        AttachCustomerCommand request, CancellationToken ct)
    {
        var transaction = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.TransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.TransactionId);

        var customer = await _uow.Customers.GetByIdAsync(request.CustomerId, ct)
            ?? throw new NotFoundException("Customer", request.CustomerId);

        if (customer.TenantId != request.TenantId)
            throw new NotFoundException("Customer", request.CustomerId);

        transaction.AttachCustomer(request.CustomerId);
        _uow.Transactions.Update(transaction);
        await _uow.SaveChangesAsync(ct);
        return CreateTransactionCommandHandler.MapToResponse(transaction, customer.Name);
    }
}
