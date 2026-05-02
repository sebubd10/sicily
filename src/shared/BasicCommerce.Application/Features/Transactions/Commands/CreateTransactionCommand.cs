using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record CreateTransactionCommand(
    Guid TenantId,
    Guid StoreId,
    Guid TerminalId,
    Guid CashierId,
    Guid? CustomerId = null) : IRequest<TransactionResponse>;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.TerminalId).NotEmpty();
        RuleFor(x => x.CashierId).NotEmpty();
    }
}

public class CreateTransactionCommandHandler
    : IRequestHandler<CreateTransactionCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;

    public CreateTransactionCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TransactionResponse> Handle(
        CreateTransactionCommand request, CancellationToken ct)
    {
        var transaction = Transaction.Create(
            request.TenantId, request.StoreId,
            request.TerminalId, request.CashierId, request.CustomerId);

        await _uow.Transactions.AddAsync(transaction, ct);
        await _uow.SaveChangesAsync(ct);

        return MapToResponse(transaction);
    }

    internal static TransactionResponse MapToResponse(Transaction t,
        string? customerName = null) =>
        new(
            Id: t.Id,
            TransactionNumber: t.TransactionNumber,
            Status: t.TransactionStatus.ToString(),
            Type: t.Type.ToString(),
            CustomerId: t.CustomerId,
            CustomerName: customerName,
            OriginalTransactionId: t.OriginalTransactionId,
            LineItems: t.LineItems.Select(l => new LineItemResponse(
                l.Id, l.ProductName, l.ProductSku, l.Quantity, l.UnitPrice,
                l.TaxRate, l.TaxAmount, l.DiscountAmount, l.LineTotal,
                l.IsVoided, l.IsPriceOverridden)),
            Payments: t.Payments.Select(p => new PaymentResponse(
                p.Id, p.Method.ToString(), p.Amount, p.PaymentStatus.ToString(), p.GatewayReference)),
            SubTotal: t.SubTotal,
            TaxTotal: t.TaxTotal,
            DiscountTotal: t.DiscountTotal,
            Total: t.Total,
            AmountPaid: t.AmountPaid,
            ChangeDue: t.ChangeDue,
            CreatedAt: t.CreatedAt,
            CompletedAt: t.CompletedAt);
}
