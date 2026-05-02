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

        throw new NotImplementedException("Implement after Infrastructure is wired up.");
    }
}
