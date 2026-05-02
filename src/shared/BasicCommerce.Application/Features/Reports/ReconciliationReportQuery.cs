using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Reports;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Reports;

public record ReconciliationReportQuery(Guid TerminalId, DateOnly Date)
    : IRequest<ReconciliationReportResponse>;

public class ReconciliationReportQueryHandler
    : IRequestHandler<ReconciliationReportQuery, ReconciliationReportResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ReconciliationReportQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ReconciliationReportResponse> Handle(
        ReconciliationReportQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var terminal = await _uow.Terminals.GetByIdAsync(request.TerminalId, ct)
            ?? throw new NotFoundException("Terminal", request.TerminalId);
        if (terminal.TenantId != tenantId)
            throw new NotFoundException("Terminal", request.TerminalId);

        var transactions = await _uow.Transactions.GetForReconciliationAsync(
            tenantId, request.TerminalId, request.Date, ct);

        var completed = transactions.Where(t => t.Status == TransactionStatus.Completed).ToList();

        var approvedPayments = completed
            .SelectMany(t => t.Payments)
            .Where(p => p.Status == PaymentStatus.Approved)
            .ToList();

        var cashSales = approvedPayments
            .Where(p => p.Method == PaymentMethod.Cash)
            .Sum(p => p.Amount);

        var paymentBreakdown = approvedPayments
            .GroupBy(p => p.Method)
            .Select(g => new PaymentMethodSummary(
                g.Key.ToString(),
                g.Count(),
                g.Sum(p => p.Amount)))
            .OrderByDescending(p => p.TotalAmount)
            .ToList();

        return new ReconciliationReportResponse(
            request.Date,
            terminal.Id,
            terminal.Name,
            terminal.Code,
            terminal.OpeningFloat,
            cashSales,
            terminal.OpeningFloat + cashSales,
            completed.Sum(t => t.Total),
            completed.Count,
            paymentBreakdown);
    }
}
