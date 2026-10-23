using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record VoidLineItemCommand(
    Guid TenantId,
    Guid TransactionId,
    Guid LineItemId,
    Guid CashierId) : IRequest<TransactionResponse>;

public class VoidLineItemCommandValidator : AbstractValidator<VoidLineItemCommand>
{
    public VoidLineItemCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.LineItemId).NotEmpty();
        RuleFor(x => x.CashierId).NotEmpty();
    }
}

public class VoidLineItemCommandHandler : IRequestHandler<VoidLineItemCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;

    public VoidLineItemCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TransactionResponse> Handle(VoidLineItemCommand request, CancellationToken ct)
    {
        var transaction = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.TransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.TransactionId);

        var line = transaction.LineItems.FirstOrDefault(l => l.Id == request.LineItemId);
        transaction.VoidLineItem(request.LineItemId, request.CashierId);

        await TransactionAudit.RecordAsync(_uow, transaction, TransactionAuditAction.ItemRemoved,
            $"Removed {line?.Quantity:0.##} × {line?.ProductName}", request.CashierId, ct: ct);

        await _uow.SaveChangesAsync(ct);

        return await CreateTransactionCommandHandler.MapToResponseAsync(_uow, transaction, ct);
    }
}
