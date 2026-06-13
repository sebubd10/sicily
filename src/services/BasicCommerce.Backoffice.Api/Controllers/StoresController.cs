using BasicCommerce.Application.Features.Stores.Commands;
using BasicCommerce.Application.Features.Stores.Queries;

using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Stores;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/stores")]
[Authorize(Policy = "StoreManagerAndAbove")]
public class StoresController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public StoresController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<StoreResponse>>>> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStoresQuery(), ct);
        return Ok(ApiResponse<IEnumerable<StoreResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StoreResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStoreQuery(id), ct);
        return Ok(ApiResponse<StoreResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<ActionResult<ApiResponse<StoreResponse>>> Create(
        [FromBody] CreateStoreRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateStoreCommand(
            request.Name,
            request.Code,
            request.AddressLine1,
            request.City,
            request.District,
            request.PostalCode,
            request.Phone,
            request.Email), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<StoreResponse>.Ok(result));
    }

    [HttpGet("{storeId:guid}/terminals")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TerminalResponse>>>> GetTerminals(
        Guid storeId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTerminalsQuery(storeId), ct);
        return Ok(ApiResponse<IEnumerable<TerminalResponse>>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<ActionResult<ApiResponse<StoreResponse>>> Update(
        Guid id, [FromBody] UpdateStoreRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateStoreCommand(
            id,
            request.Name,
            request.AddressLine1,
            request.City,
            request.District,
            request.PostalCode,
            request.AddressLine2,
            request.Phone,
            request.Email,
            request.OpeningTime,
            request.ClosingTime), ct);
        return Ok(ApiResponse<StoreResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateStoreCommand(id), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/activate")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ActivateStoreCommand(id), ct);
        return NoContent();
    }
}
