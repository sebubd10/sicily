using BasicCommerce.Application.Features.Reports;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Backoffice.Api.Attributes;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Reports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static BasicCommerce.Application.Features.Auth.PermissionCodes;

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
    public async Task<ActionResult<ApiResponse<DailySalesReportResponse>>> DailySales(
        [FromQuery] DateOnly? date,
        [FromQuery] Guid? storeId,
        CancellationToken ct)
    {
        var reportDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await _mediator.Send(new DailySalesReportQuery(reportDate, storeId), ct);
        return Ok(ApiResponse<DailySalesReportResponse>.Ok(result));
    }

    /// <summary>Cashier performance — transactions, voids, overrides per cashier.</summary>
    [HttpGet("cashier-performance")]
    public async Task<ActionResult<ApiResponse<CashierPerformanceReportResponse>>> CashierPerformance(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? storeId,
        CancellationToken ct)
    {
        var start = from ?? DateTime.UtcNow.Date;
        var end = to ?? DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);
        var result = await _mediator.Send(new CashierPerformanceReportQuery(start, end, storeId), ct);
        return Ok(ApiResponse<CashierPerformanceReportResponse>.Ok(result));
    }

    /// <summary>End-of-day cash reconciliation for a terminal.</summary>
    [HttpGet("reconciliation/{terminalId:guid}")]
    public async Task<ActionResult<ApiResponse<ReconciliationReportResponse>>> Reconciliation(
        Guid terminalId,
        [FromQuery] DateOnly? date,
        CancellationToken ct)
    {
        var reportDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await _mediator.Send(new ReconciliationReportQuery(terminalId, reportDate), ct);
        return Ok(ApiResponse<ReconciliationReportResponse>.Ok(result));
    }

    /// <summary>Stock levels and low-stock alerts for a store.</summary>
    [HttpGet("stock/{storeId:guid}")]
    public async Task<ActionResult<ApiResponse<StockReportResponse>>> StockReport(
        Guid storeId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new StockReportQuery(storeId), ct);
        return Ok(ApiResponse<StockReportResponse>.Ok(result));
    }

    /// <summary>Downloads a PDF report listing all categories for the current tenant.</summary>
    [HttpGet("categories/pdf")]
    [HasPermission(Reports.CategoryReport)]
    public async Task<IActionResult> CategoryReportPdf(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var pdf = await _mediator.Send(new CategoryReportQuery(includeInactive, search), ct);
        var filename = $"categories_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        return File(pdf, "application/pdf", filename);
    }

    /// <summary>Downloads a PDF report listing all users and their user types for the current tenant.</summary>
    [HttpGet("users/pdf")]
    [HasPermission(Reports.UserReport)]
    public async Task<IActionResult> UserReportPdf(
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var pdf = await _mediator.Send(new UserReportQuery(search), ct);
        var filename = $"users_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        return File(pdf, "application/pdf", filename);
    }

    /// <summary>Downloads a PDF report listing all manufacturers for the current tenant.</summary>
    [HttpGet("manufacturers/pdf")]
    [HasPermission(Reports.ManufacturerReport)]
    public async Task<IActionResult> ManufacturerReportPdf(
        [FromQuery] string? search = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var pdf = await _mediator.Send(new ManufacturerReportQuery(search, includeInactive), ct);
        var filename = $"manufacturers_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        return File(pdf, "application/pdf", filename);
    }
}
