using BasicCommerce.Application.Features.Till.Commands;
using BasicCommerce.Application.Features.Till.Queries;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Till;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Pos.Api.Controllers;

[ApiController]
[Route("api/till")]
[Authorize]
public class TillController : ControllerBase
{
    private readonly IMediator _mediator;

    public TillController(IMediator mediator) => _mediator = mediator;

    [HttpGet("session")]
    public async Task<ActionResult<ApiResponse<TillSessionResponse?>>> GetOpen(
        [FromQuery] Guid terminalId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOpenTillSessionQuery(terminalId), ct);
        return Ok(ApiResponse<TillSessionResponse?>.Ok(result));
    }

    [HttpGet("session/{id:guid}")]
    public async Task<ActionResult<ApiResponse<TillSessionResponse>>> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTillSessionQuery(id), ct);
        return Ok(ApiResponse<TillSessionResponse>.Ok(result));
    }

    [HttpGet("session/{id:guid}/x-report")]
    public async Task<ActionResult<ApiResponse<TillReportResponse>>> XReport(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetXReportQuery(id), ct);
        return Ok(ApiResponse<TillReportResponse>.Ok(result));
    }

    [HttpPost("session/open")]
    public async Task<ActionResult<ApiResponse<TillSessionResponse>>> Open(
        [FromBody] OpenTillSessionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new OpenTillSessionCommand(
            request.StoreId, request.TerminalId, request.OpeningFloat, request.Notes), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<TillSessionResponse>.Ok(result));
    }

    [HttpPost("session/{id:guid}/close")]
    public async Task<ActionResult<ApiResponse<TillReportResponse>>> Close(
        Guid id, [FromBody] CloseTillSessionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CloseTillSessionCommand(id, request.ClosingBalance, request.Notes), ct);
        return Ok(ApiResponse<TillReportResponse>.Ok(result));
    }

    [HttpPost("session/{id:guid}/cash-in")]
    public async Task<ActionResult<ApiResponse<TillSessionResponse>>> CashIn(
        Guid id, [FromBody] PettyCashRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new PettyCashInCommand(id, request.Amount, request.Reason), ct);
        return Ok(ApiResponse<TillSessionResponse>.Ok(result));
    }

    [HttpPost("session/{id:guid}/cash-out")]
    public async Task<ActionResult<ApiResponse<TillSessionResponse>>> CashOut(
        Guid id, [FromBody] PettyCashRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new PettyCashOutCommand(id, request.Amount, request.Reason), ct);
        return Ok(ApiResponse<TillSessionResponse>.Ok(result));
    }
}
