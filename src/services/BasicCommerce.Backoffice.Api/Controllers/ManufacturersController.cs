using BasicCommerce.Application.Common;
using BasicCommerce.Application.Features.Manufacturers.Commands;
using BasicCommerce.Application.Features.Manufacturers.Queries;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Manufacturers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/manufacturers")]
[Authorize]
public class ManufacturersController : ControllerBase
{
    private readonly IMediator _mediator;

    public ManufacturersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("countries")]
    public ActionResult<ApiResponse<IEnumerable<CountryResponse>>> GetCountries()
    {
        var countries = Countries.All
            .Select(kv => new CountryResponse(kv.Key, kv.Value));
        return Ok(ApiResponse<IEnumerable<CountryResponse>>.Ok(countries));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ManufacturerResponse>>>> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetManufacturersQuery(), ct);
        return Ok(ApiResponse<IEnumerable<ManufacturerResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ManufacturerResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetManufacturerQuery(id), ct);
        return Ok(ApiResponse<ManufacturerResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<ManufacturerResponse>>> Create(
        [FromBody] CreateManufacturerRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateManufacturerCommand(
            request.Name, request.Code, request.Country,
            request.Website, request.ContactEmail, request.Notes), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ManufacturerResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<ManufacturerResponse>>> Update(
        Guid id, [FromBody] UpdateManufacturerRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateManufacturerCommand(
            id, request.Name, request.Code, request.Country,
            request.Website, request.ContactEmail, request.Notes), ct);
        return Ok(ApiResponse<ManufacturerResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateManufacturerCommand(id), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/activate")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ActivateManufacturerCommand(id), ct);
        return NoContent();
    }
}
