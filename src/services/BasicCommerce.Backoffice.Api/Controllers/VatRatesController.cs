using BasicCommerce.Application.Features.Products.Commands;
using BasicCommerce.Application.Features.Products.Queries;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/vat-rates")]
[Authorize]
public class VatRatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VatRatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<VatRateResponse>>>> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVatRatesQuery(), ct);
        return Ok(ApiResponse<IEnumerable<VatRateResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<VatRateResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVatRateQuery(id), ct);
        return Ok(ApiResponse<VatRateResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<ActionResult<ApiResponse<VatRateResponse>>> Create(
        [FromBody] CreateVatRateRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateVatRateCommand(
            request.Name, request.Code, request.Rate, request.IsDefault), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<VatRateResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<ActionResult<ApiResponse<VatRateResponse>>> Update(
        Guid id, [FromBody] UpdateVatRateRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateVatRateCommand(
            id, request.Name, request.Code, request.Rate, request.IsDefault), ct);
        return Ok(ApiResponse<VatRateResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateVatRateCommand(id), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/activate")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ActivateVatRateCommand(id), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteVatRateCommand(id), ct);
        return NoContent();
    }
}
