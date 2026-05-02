using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Policy = "StoreManagerAndAbove")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public ReportsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>Daily sales summary — total revenue, tax, transactions by payment method.</summary>
    [HttpGet("daily-sales")]
    public IActionResult DailySales([FromQuery] DateTime? date, [FromQuery] Guid? storeId) =>
        StatusCode(501, ApiResponse<object>.Fail("DailySalesReportQuery not yet implemented."));

    /// <summary>Cashier performance — transactions, voids, overrides per cashier.</summary>
    [HttpGet("cashier-performance")]
    public IActionResult CashierPerformance(
        [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] Guid? storeId) =>
        StatusCode(501, ApiResponse<object>.Fail("CashierPerformanceReportQuery not yet implemented."));

    /// <summary>End-of-day cash reconciliation for a terminal.</summary>
    [HttpGet("reconciliation/{terminalId:guid}")]
    public IActionResult Reconciliation(Guid terminalId, [FromQuery] DateTime? date) =>
        StatusCode(501, ApiResponse<object>.Fail("ReconciliationReportQuery not yet implemented."));

    /// <summary>Stock levels and low-stock alerts for a store.</summary>
    [HttpGet("stock/{storeId:guid}")]
    public IActionResult StockReport(Guid storeId) =>
        StatusCode(501, ApiResponse<object>.Fail("StockReportQuery not yet implemented."));
}
