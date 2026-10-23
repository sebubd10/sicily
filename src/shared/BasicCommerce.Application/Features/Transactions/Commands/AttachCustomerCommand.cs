using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record AttachCustomerCommand(Guid TenantId, Guid TransactionId, Guid CustomerId)
    : IRequest<TransactionResponse>;

public class AttachCustomerCommandHandler : IRequestHandler<AttachCustomerCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public AttachCustomerCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TransactionResponse> Handle(
        AttachCustomerCommand request, CancellationToken ct)
    {
        var transaction = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.TransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.TransactionId);

        var customer = await _uow.Customers.GetByIdForTenantAsync(request.TenantId, request.CustomerId, ct)
            ?? throw new NotFoundException("Customer", request.CustomerId);

        if (transaction.CustomerId == customer.Id)
            return CreateTransactionCommandHandler.MapToResponse(transaction, customer.Name);

        var previous = transaction.CustomerId.HasValue
            ? (await _uow.Customers.GetByIdAsync(transaction.CustomerId.Value, ct))?.Name
            : null;

        transaction.AttachCustomer(customer.Id);

        await TransactionAudit.RecordAsync(_uow, transaction, TransactionAuditAction.CustomerChanged,
            previous is null ? $"Customer set to {customer.Name}" : $"Customer changed {previous} → {customer.Name}",
            _currentUser.UserId, ct: ct);

        await _uow.SaveChangesAsync(ct);

        return CreateTransactionCommandHandler.MapToResponse(transaction, customer.Name);
    }
}

/// <summary>Removes the customer from an unfinished sale (back to walk-in).</summary>
public record DetachCustomerCommand(Guid TenantId, Guid TransactionId) : IRequest<TransactionResponse>;

public class DetachCustomerCommandHandler : IRequestHandler<DetachCustomerCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DetachCustomerCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TransactionResponse> Handle(DetachCustomerCommand request, CancellationToken ct)
    {
        var transaction = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.TransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.TransactionId);

        if (!transaction.CustomerId.HasValue)
            return CreateTransactionCommandHandler.MapToResponse(transaction);

        var previous = (await _uow.Customers.GetByIdAsync(transaction.CustomerId.Value, ct))?.Name;
        transaction.DetachCustomer();

        await TransactionAudit.RecordAsync(_uow, transaction, TransactionAuditAction.CustomerChanged,
            previous is null ? "Customer removed — now walk-in" : $"Customer {previous} removed — now walk-in",
            _currentUser.UserId, ct: ct);

        await _uow.SaveChangesAsync(ct);
        return CreateTransactionCommandHandler.MapToResponse(transaction);
    }
}
