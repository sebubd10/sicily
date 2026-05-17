using BasicCommerce.Application.Common;
using BasicCommerce.Application.Features.Warehouses.Commands;
using BasicCommerce.Application.Features.Warehouses.Queries;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Warehouses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/warehouses")]
[Authorize]
public class WarehousesController : ControllerBase
{
    private readonly IMediator _mediator;

    public WarehousesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("districts")]
    public ActionResult<ApiResponse<IEnumerable<DistrictResponse>>> GetDistricts()
    {
        var districts = BangladeshDistricts.All
            .OrderBy(kv => kv.Value)
            .Select(kv => new DistrictResponse(kv.Key, kv.Value));
        return Ok(ApiResponse<IEnumerable<DistrictResponse>>.Ok(districts));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<WarehouseResponse>>>> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetWarehousesQuery(), ct);
        return Ok(ApiResponse<IEnumerable<WarehouseResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<WarehouseResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetWarehouseQuery(id), ct);
        return Ok(ApiResponse<WarehouseResponse>.Ok(result));
    }

    [HttpGet("{id:guid}/stock")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WarehouseStockLevelResponse>>>> GetStock(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetWarehouseStockQuery(id), ct);
        return Ok(ApiResponse<IEnumerable<WarehouseStockLevelResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}/movements")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WarehouseMovementResponse>>>> GetMovements(
        Guid id, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int limit = 200, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetWarehouseMovementsQuery(id, from, to, limit), ct);
        return Ok(ApiResponse<IEnumerable<WarehouseMovementResponse>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<WarehouseResponse>>> Create(
        [FromBody] CreateWarehouseRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateWarehouseCommand(
            request.Name, request.Code, request.AddressLine1, request.AddressLine2,
            request.City, request.District, request.PostalCode, request.Country,
            request.Phone, request.Email, request.IsDefault), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<WarehouseResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<WarehouseResponse>>> Update(
        Guid id, [FromBody] UpdateWarehouseRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateWarehouseCommand(
            id, request.Name, request.AddressLine1, request.AddressLine2,
            request.City, request.District, request.PostalCode, request.Country,
            request.Phone, request.Email), ct);
        return Ok(ApiResponse<WarehouseResponse>.Ok(result));
    }

    [HttpPost("transfer-to-store")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> TransferToStore(
        [FromBody] TransferWarehouseToStoreRequest request, CancellationToken ct)
    {
        await _mediator.Send(new TransferWarehouseToStoreCommand(
            request.WarehouseId, request.StoreId, request.ProductId,
            request.Quantity, request.Notes), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateWarehouseCommand(id), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/activate")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ActivateWarehouseCommand(id), ct);
        return NoContent();
    }
}
