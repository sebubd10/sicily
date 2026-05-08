using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Till;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Till.Commands;

public record CloseTillSessionCommand(
    Guid SessionId,
    decimal ClosingBalance,
    string? Notes) : IRequest<TillReportResponse>;

public class CloseTillSessionCommandValidator : AbstractValidator<CloseTillSessionCommand>
{
    public CloseTillSessionCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.ClosingBalance).GreaterThanOrEqualTo(0);
    }
}

public class CloseTillSessionCommandHandler
    : IRequestHandler<CloseTillSessionCommand, TillReportResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CloseTillSessionCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TillReportResponse> Handle(
        CloseTillSessionCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var session = await _uow.TillSessions.GetWithPettyAsync(tenantId, request.SessionId, ct)
            ?? throw new NotFoundException("TillSession", request.SessionId);

        var (cashSales, cardSales, giftCardSales, refunds) =
            await GetSessionTotalsAsync(tenantId, session.TerminalId,
                session.OpenedAt, ct);

        var snapshot = session.Close(_currentUser.UserId, request.ClosingBalance,
            cashSales, cardSales, giftCardSales, refunds, request.Notes);

        _uow.TillSessions.Update(session);
        await _uow.SaveChangesAsync(ct);

        return TillMapper.ToReport(snapshot, request.ClosingBalance);
    }

    private async Task<(decimal cash, decimal card, decimal giftCard, decimal refunds)>
        GetSessionTotalsAsync(Guid tenantId, Guid terminalId, DateTime from, CancellationToken ct)
    {
        var txns = await _uow.Transactions.GetByTerminalAsync(
            tenantId, terminalId, from, DateTime.UtcNow, ct);

        var completed = txns.Where(t => t.TransactionStatus == Domain.Enums.TransactionStatus.Completed).ToList();
        var returns = txns.Where(t => t.Type == Domain.Enums.TransactionType.Return
            && t.TransactionStatus == Domain.Enums.TransactionStatus.Completed).ToList();

        var cashSales = completed
            .Where(t => t.Type != Domain.Enums.TransactionType.Return)
            .SelectMany(t => t.Payments)
            .Where(p => p.PaymentStatus == Domain.Enums.PaymentStatus.Approved
                && p.Method == Domain.Enums.PaymentMethod.Cash)
            .Sum(p => p.Amount);

        var cardSales = completed
            .Where(t => t.Type != Domain.Enums.TransactionType.Return)
            .SelectMany(t => t.Payments)
            .Where(p => p.PaymentStatus == Domain.Enums.PaymentStatus.Approved
                && p.Method is Domain.Enums.PaymentMethod.Card
                    or Domain.Enums.PaymentMethod.BKash
                    or Domain.Enums.PaymentMethod.Nagad
                    or Domain.Enums.PaymentMethod.Rocket)
            .Sum(p => p.Amount);

        var giftCardSales = completed
            .Where(t => t.Type != Domain.Enums.TransactionType.Return)
            .SelectMany(t => t.Payments)
            .Where(p => p.PaymentStatus == Domain.Enums.PaymentStatus.Approved
                && p.Method == Domain.Enums.PaymentMethod.GiftCard)
            .Sum(p => p.Amount);

        var refunds = returns.Sum(t => t.Total);

        return (cashSales, cardSales, giftCardSales, refunds);
    }
}
