using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record ApplyLineItemDiscountCommand(
    Guid TenantId,
    Guid TransactionId,
    Guid LineItemId,
    decimal DiscountAmount) : IRequest<TransactionResponse>;

public class ApplyLineItemDiscountCommandValidator
    : AbstractValidator<ApplyLineItemDiscountCommand>
{
    public ApplyLineItemDiscountCommandValidator()
    {
        RuleFor(x => x.DiscountAmount).GreaterThan(0);
    }
}

public class ApplyLineItemDiscountCommandHandler
    : IRequestHandler<ApplyLineItemDiscountCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;
    public ApplyLineItemDiscountCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TransactionResponse> Handle(
        ApplyLineItemDiscountCommand request, CancellationToken ct)
    {
        var transaction = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.TransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.TransactionId);

        transaction.ApplyLineItemDiscount(request.LineItemId, request.DiscountAmount);
        _uow.Transactions.Update(transaction);
        await _uow.SaveChangesAsync(ct);
        return CreateTransactionCommandHandler.MapToResponse(transaction);
    }
}
