using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Reports;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Reports;

public record CashierPerformanceReportQuery(DateTime From, DateTime To, Guid? StoreId)
    : IRequest<CashierPerformanceReportResponse>;

public class CashierPerformanceReportQueryHandler
    : IRequestHandler<CashierPerformanceReportQuery, CashierPerformanceReportResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CashierPerformanceReportQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<CashierPerformanceReportResponse> Handle(
        CashierPerformanceReportQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var transactions = await _uow.Transactions.GetForDateRangeReportAsync(
            tenantId, request.StoreId, request.From, request.To, ct);

        var cashierIds = transactions.Select(t => t.CashierId).Distinct().ToList();
        var cashierNames = new Dictionary<Guid, string>();
        foreach (var id in cashierIds)
        {
            var user = await _uow.Users.GetByIdAsync(id, ct);
            cashierNames[id] = user?.FullName ?? id.ToString("N")[..8];
        }

        var grouped = transactions.GroupBy(t => t.CashierId);
        var summaries = grouped.Select(g =>
        {
            var completed = g.Where(t => t.TransactionStatus == TransactionStatus.Completed).ToList();
            var voided = g.Where(t => t.TransactionStatus == TransactionStatus.Voided).ToList();
            var revenue = completed.Sum(t => t.Total);
            var overrides = g.SelectMany(t => t.LineItems).Count(l => l.IsPriceOverridden);

            return new CashierPerformanceSummary(
                g.Key,
                cashierNames.GetValueOrDefault(g.Key, "Unknown"),
                completed.Count,
                voided.Count,
                overrides,
                revenue,
                completed.Count > 0 ? Math.Round(revenue / completed.Count, 2) : 0);
        })
        .OrderByDescending(s => s.TotalRevenue)
        .ToList();

        return new CashierPerformanceReportResponse(request.From, request.To, summaries);
    }
}
