using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Reports;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Reports;

public record DailySalesReportQuery(DateOnly Date, Guid? StoreId) : IRequest<DailySalesReportResponse>;

public class DailySalesReportQueryHandler : IRequestHandler<DailySalesReportQuery, DailySalesReportResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DailySalesReportQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<DailySalesReportResponse> Handle(DailySalesReportQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        string? storeName = null;
        if (request.StoreId.HasValue)
        {
            var store = await _uow.Stores.GetByIdAsync(request.StoreId.Value, ct)
                ?? throw new NotFoundException("Store", request.StoreId.Value);
            if (store.TenantId != tenantId)
                throw new NotFoundException("Store", request.StoreId.Value);
            storeName = store.Name;
        }

        var transactions = await _uow.Transactions.GetForDailyReportAsync(
            tenantId, request.StoreId, request.Date, ct);

        var completed = transactions.Where(t => t.TransactionStatus == TransactionStatus.Completed).ToList();
        var voided = transactions.Where(t => t.TransactionStatus == TransactionStatus.Voided).ToList();

        var grossRevenue = completed.Sum(t => t.Total);
        var taxTotal = completed.Sum(t => t.TaxTotal);
        var discountTotal = completed.Sum(t => t.DiscountTotal);
        var netRevenue = grossRevenue - taxTotal;

        var paymentBreakdown = completed
            .SelectMany(t => t.Payments)
            .Where(p => p.PaymentStatus == PaymentStatus.Approved)
            .GroupBy(p => p.Method)
            .Select(g => new PaymentMethodSummary(
                g.Key.ToString(),
                g.Count(),
                g.Sum(p => p.Amount)))
            .OrderByDescending(p => p.TotalAmount)
            .ToList();

        return new DailySalesReportResponse(
            request.Date,
            request.StoreId,
            storeName,
            completed.Count,
            voided.Count,
            grossRevenue,
            taxTotal,
            discountTotal,
            netRevenue,
            paymentBreakdown);
    }
}
