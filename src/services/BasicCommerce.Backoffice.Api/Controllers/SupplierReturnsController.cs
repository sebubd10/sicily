using BasicCommerce.Application.Features.SupplierReturns.Commands;
using BasicCommerce.Application.Features.SupplierReturns.Queries;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.SupplierReturns;
using BasicCommerce.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/supplier-returns")]
[Authorize(Policy = "StoreManagerAndAbove")]
public class SupplierReturnsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SupplierReturnsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<SupplierReturnListResponse>>> GetAll(
        [FromQuery] Guid? supplierId,
        [FromQuery] Guid? storeId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var statusEnum = Enum.TryParse<SupplierReturnStatus>(status, true, out var s)
            ? s : (SupplierReturnStatus?)null;

        var result = await _mediator.Send(
            new GetSupplierReturnsQuery(supplierId, storeId, statusEnum, page, pageSize), ct);
        return Ok(ApiResponse<SupplierReturnListResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SupplierReturnResponse>>> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSupplierReturnQuery(id), ct);
        return Ok(ApiResponse<SupplierReturnResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SupplierReturnResponse>>> Create(
        [FromBody] CreateSupplierReturnRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateSupplierReturnCommand(
            request.SupplierId, request.StoreId,
            request.PurchaseOrderId, request.Notes), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<SupplierReturnResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/items")]
    public async Task<ActionResult<ApiResponse<SupplierReturnResponse>>> AddItem(
        Guid id, [FromBody] AddSupplierReturnItemRequest request, CancellationToken ct)
    {
        var reason = Enum.TryParse<SupplierReturnReason>(request.Reason, true, out var r)
            ? r : SupplierReturnReason.Damaged;

        var result = await _mediator.Send(new AddSupplierReturnItemCommand(
            id, request.ProductId, request.Quantity,
            request.UnitCost, reason, request.Notes), ct);
        return Ok(ApiResponse<SupplierReturnResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}/items/{productId:guid}")]
    public async Task<ActionResult<ApiResponse<SupplierReturnResponse>>> RemoveItem(
        Guid id, Guid productId, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new RemoveSupplierReturnItemCommand(id, productId), ct);
        return Ok(ApiResponse<SupplierReturnResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/expected-credit")]
    public async Task<ActionResult<ApiResponse<SupplierReturnResponse>>> SetExpectedCredit(
        Guid id, [FromBody] SetExpectedCreditRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new SetExpectedCreditCommand(id, request.ExpectedCreditAmount), ct);
        return Ok(ApiResponse<SupplierReturnResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult<ApiResponse<SupplierReturnResponse>>> Submit(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new SubmitSupplierReturnCommand(id), ct);
        return Ok(ApiResponse<SupplierReturnResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/ship")]
    public async Task<ActionResult<ApiResponse<SupplierReturnResponse>>> Ship(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ShipSupplierReturnCommand(id), ct);
        return Ok(ApiResponse<SupplierReturnResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/receive-credit")]
    public async Task<ActionResult<ApiResponse<SupplierReturnResponse>>> ReceiveCredit(
        Guid id, [FromBody] ReceiveCreditRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ReceiveCreditCommand(id, request.CreditAmount, request.CreditNoteReference), ct);
        return Ok(ApiResponse<SupplierReturnResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<SupplierReturnResponse>>> Cancel(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelSupplierReturnCommand(id), ct);
        return Ok(ApiResponse<SupplierReturnResponse>.Ok(result));
    }
}
