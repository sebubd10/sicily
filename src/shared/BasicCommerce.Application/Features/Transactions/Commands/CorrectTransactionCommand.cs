using BasicCommerce.Application.Features.RewardPoints;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Transactions.Commands;

public record PaymentMethodCorrection(Guid PaymentId, PaymentMethod NewMethod, string? MobileNumber);

/// <summary>
/// Non-financial corrections to a completed sale. Anything that changes what was sold or
/// charged goes through a return, exchange or void instead.
/// </summary>
public record CorrectTransactionCommand(
    Guid TenantId,
    Guid TransactionId,
    string Reason,
    bool UpdateNotes,
    string? Notes,
    Guid? AttachCustomerId,
    IReadOnlyList<PaymentMethodCorrection> PaymentCorrections) : IRequest<TransactionResponse>;

public class CorrectTransactionCommandValidator : AbstractValidator<CorrectTransactionCommand>
{
    public CorrectTransactionCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Give a reason for the correction.").MaximumLength(500);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x)
            .Must(x => x.UpdateNotes || x.AttachCustomerId.HasValue || x.PaymentCorrections.Count > 0)
            .WithMessage("Nothing to correct.");
    }
}

public class CorrectTransactionCommandHandler
    : IRequestHandler<CorrectTransactionCommand, TransactionResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CorrectTransactionCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TransactionResponse> Handle(CorrectTransactionCommand request, CancellationToken ct)
    {
        if ((int)_currentUser.Role < (int)UserRole.Supervisor)
            throw new UnauthorizedException("Only a supervisor or above can correct a completed sale.");

        await _uow.BeginTransactionAsync(ct);
        try
        {
            var transaction = await _uow.Transactions.GetWithItemsAsync(
                request.TenantId, request.TransactionId, ct)
                ?? throw new NotFoundException("Transaction", request.TransactionId);

            if (transaction.TransactionStatus == TransactionStatus.Voided)
                throw new DomainException("A voided sale can't be corrected.");

            var userId = _currentUser.UserId;

            if (request.UpdateNotes && (request.Notes ?? "") != (transaction.Notes ?? ""))
            {
                transaction.UpdateNotes(request.Notes);
                await TransactionAudit.RecordAsync(_uow, transaction, TransactionAuditAction.NotesChanged,
                    string.IsNullOrWhiteSpace(request.Notes) ? "Notes cleared" : $"Notes set to \"{request.Notes!.Trim()}\"",
                    userId, request.Reason, ct);
            }

            if (request.AttachCustomerId is Guid customerId)
            {
                var customer = await _uow.Customers.GetByIdForTenantAsync(request.TenantId, customerId, ct)
                    ?? throw new NotFoundException("Customer", customerId);
                if (customer.Status != EntityStatus.Active)
                    throw new DomainException($"Customer '{customer.Name}' is inactive.");

                transaction.AttachCustomerAfterCompletion(customer.Id);
                var points = await RewardPointsLedger.AwardPurchasePointsAsync(_uow, transaction, ct);

                await TransactionAudit.RecordAsync(_uow, transaction, TransactionAuditAction.CustomerChanged,
                    $"Customer {customer.Name} added after the sale" + (points > 0 ? $" (+{points} pts awarded)" : ""),
                    userId, request.Reason, ct);
            }

            if (request.PaymentCorrections.Count > 0)
            {
                await TillRules.EnsureSaleInOpenTillSessionAsync(_uow, transaction,
                    "have its payment method changed", ct);

                foreach (var fix in request.PaymentCorrections)
                {
                    if (fix.NewMethod is PaymentMethod.BKash or PaymentMethod.Nagad or PaymentMethod.Rocket
                        && string.IsNullOrWhiteSpace(fix.MobileNumber))
                        throw new DomainException($"Mobile number is required for {fix.NewMethod}.");

                    var oldMethod = transaction.Payments.FirstOrDefault(p => p.Id == fix.PaymentId)?.Method;
                    if (oldMethod == fix.NewMethod) continue;

                    var payment = transaction.CorrectPaymentMethod(fix.PaymentId, fix.NewMethod, fix.MobileNumber);
                    await TransactionAudit.RecordAsync(_uow, transaction, TransactionAuditAction.PaymentMethodCorrected,
                        $"Payment of {TransactionAudit.Money(payment.Amount)} changed {oldMethod} → {fix.NewMethod}" +
                        (fix.MobileNumber is not null ? $" ({fix.MobileNumber})" : ""),
                        userId, request.Reason, ct);
                }
            }

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            return await CreateTransactionCommandHandler.MapToResponseAsync(_uow, transaction, ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
