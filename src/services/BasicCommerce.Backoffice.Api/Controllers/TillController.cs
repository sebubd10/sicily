using BasicCommerce.Application.Features.Till.Commands;
using BasicCommerce.Application.Features.Till.Queries;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Till;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/till-sessions")]
[Authorize(Policy = "StoreManagerAndAbove")]
public class TillController : ControllerBase
{
    private readonly IMediator _mediator;

    public TillController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<TillSessionListResponse>>> GetAll(
        [FromQuery] Guid? storeId,
        [FromQuery] Guid? terminalId,
        [FromQuery] bool openOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetTillSessionsQuery(storeId, terminalId, openOnly, page, pageSize), ct);
        return Ok(ApiResponse<TillSessionListResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TillSessionResponse>>> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTillSessionQuery(id), ct);
        return Ok(ApiResponse<TillSessionResponse>.Ok(result));
    }

    [HttpGet("open")]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<ActionResult<ApiResponse<TillSessionResponse?>>> GetOpen(
        [FromQuery] Guid terminalId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOpenTillSessionQuery(terminalId), ct);
        return Ok(ApiResponse<TillSessionResponse?>.Ok(result));
    }

    [HttpGet("{id:guid}/x-report")]
    public async Task<ActionResult<ApiResponse<TillReportResponse>>> XReport(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetXReportQuery(id), ct);
        return Ok(ApiResponse<TillReportResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<ActionResult<ApiResponse<TillSessionResponse>>> Open(
        [FromBody] OpenTillSessionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new OpenTillSessionCommand(
            request.StoreId, request.TerminalId, request.OpeningFloat, request.Notes), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<TillSessionResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/close")]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<ActionResult<ApiResponse<TillReportResponse>>> Close(
        Guid id, [FromBody] CloseTillSessionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CloseTillSessionCommand(id, request.ClosingBalance, request.Notes), ct);
        return Ok(ApiResponse<TillReportResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/cash-in")]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<ActionResult<ApiResponse<TillSessionResponse>>> CashIn(
        Guid id, [FromBody] PettyCashRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new PettyCashInCommand(id, request.Amount, request.Reason), ct);
        return Ok(ApiResponse<TillSessionResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/cash-out")]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<ActionResult<ApiResponse<TillSessionResponse>>> CashOut(
        Guid id, [FromBody] PettyCashRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new PettyCashOutCommand(id, request.Amount, request.Reason), ct);
        return Ok(ApiResponse<TillSessionResponse>.Ok(result));
    }
}
