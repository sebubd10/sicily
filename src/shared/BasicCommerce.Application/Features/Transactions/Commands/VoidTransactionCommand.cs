using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record VoidTransactionCommand(
    Guid TenantId,
    Guid TransactionId,
    Guid SupervisorId,
    string Reason) : IRequest<TransactionResponse>;

public class VoidTransactionCommandValidator : AbstractValidator<VoidTransactionCommand>
{
    public VoidTransactionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.SupervisorId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class VoidTransactionCommandHandler
    : IRequestHandler<VoidTransactionCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;

    public VoidTransactionCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TransactionResponse> Handle(
        VoidTransactionCommand request, CancellationToken ct)
    {
        var transaction = await _uow.Transactions.GetWithItemsAsync(
            request.TenantId, request.TransactionId, ct)
            ?? throw new NotFoundException("Transaction", request.TransactionId);

        var supervisor = await _uow.Users.GetByIdForTenantAsync(
            request.TenantId, request.SupervisorId, ct)
            ?? throw new NotFoundException("Supervisor", request.SupervisorId);

        if ((int)supervisor.Role < (int)Domain.Enums.UserRole.Supervisor)
            throw new UnauthorizedException("Only a supervisor or above can void transactions.");

        transaction.Void(request.SupervisorId, request.Reason);

        _uow.Transactions.Update(transaction);
        await _uow.SaveChangesAsync(ct);

        return CreateTransactionCommandHandler.MapToResponse(transaction);
    }
}
