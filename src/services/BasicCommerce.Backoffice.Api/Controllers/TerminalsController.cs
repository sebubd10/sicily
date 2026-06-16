using BasicCommerce.Application.Features.Terminals.Commands;
using BasicCommerce.Application.Features.Terminals.Queries;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Stores;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/terminals")]
[Authorize(Policy = "StoreManagerAndAbove")]
public class TerminalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TerminalsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<TerminalResponse>>>> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllTerminalsQuery(), ct);
        return Ok(ApiResponse<IEnumerable<TerminalResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TerminalResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTerminalQuery(id), ct);
        return Ok(ApiResponse<TerminalResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<ActionResult<ApiResponse<TerminalResponse>>> Create(
        [FromBody] CreateTerminalRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateTerminalCommand(
            request.StoreId, request.Name, request.Code, request.Type), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<TerminalResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<ActionResult<ApiResponse<TerminalResponse>>> Update(
        Guid id, [FromBody] UpdateTerminalRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateTerminalCommand(
            id, request.StoreId, request.Name, request.Code, request.Type), ct);
        return Ok(ApiResponse<TerminalResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateTerminalCommand(id), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/activate")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ActivateTerminalCommand(id), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteTerminalCommand(id), ct);
        return NoContent();
    }
}
