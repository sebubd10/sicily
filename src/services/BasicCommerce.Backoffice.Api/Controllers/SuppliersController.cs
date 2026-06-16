using BasicCommerce.Application.Features.Suppliers.Commands;
using BasicCommerce.Application.Features.Suppliers.Queries;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Suppliers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SuppliersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<SupplierResponse>>>> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSuppliersQuery(), ct);
        return Ok(ApiResponse<IEnumerable<SupplierResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SupplierResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSupplierQuery(id), ct);
        return Ok(ApiResponse<SupplierResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<SupplierResponse>>> Create(
        [FromBody] CreateSupplierRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateSupplierCommand(
            request.Name, request.Code, request.ContactName, request.Email, request.Phone,
            request.AddressLine1, request.AddressLine2, request.City, request.District,
            request.PostalCode, request.Country, request.LeadTimeDays,
            request.Notes, request.ManufacturerId), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<SupplierResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<SupplierResponse>>> Update(
        Guid id, [FromBody] UpdateSupplierRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateSupplierCommand(
            id, request.Name, request.ContactName, request.Email, request.Phone,
            request.AddressLine1, request.AddressLine2, request.City, request.District,
            request.PostalCode, request.Country, request.LeadTimeDays,
            request.Notes, request.ManufacturerId), ct);
        return Ok(ApiResponse<SupplierResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateSupplierCommand(id), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/activate")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ActivateSupplierCommand(id), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteSupplierCommand(id), ct);
        return NoContent();
    }
}
