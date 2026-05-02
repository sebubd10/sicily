using BasicCommerce.Contracts.Transactions;
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

        transaction.VoidLineItem(request.LineItemId, request.CashierId);

        _uow.Transactions.Update(transaction);
        await _uow.SaveChangesAsync(ct);

        return CreateTransactionCommandHandler.MapToResponse(transaction);
    }
}
