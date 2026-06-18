using BasicCommerce.Application.Features.PurchaseOrders.Commands;
using BasicCommerce.Application.Features.PurchaseOrders.Queries;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.PurchaseOrders;
using BasicCommerce.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/purchase-orders")]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public PurchaseOrdersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PurchaseOrderListResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] PurchaseOrderStatus? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? dateField = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetPurchaseOrdersQuery(page, pageSize, supplierId, warehouseId, status, from, to, dateField), ct);
        return Ok(ApiResponse<PurchaseOrderListResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPurchaseOrderQuery(id), ct);
        return Ok(ApiResponse<PurchaseOrderResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Create(
        [FromBody] CreatePurchaseOrderRequest request, CancellationToken ct)
    {
        var items = request.Items?.Select(i => (i.ProductId, i.Quantity, i.UnitCost))
            ?? Enumerable.Empty<(Guid, decimal, decimal?)>();
        var result = await _mediator.Send(new CreatePurchaseOrderCommand(
            request.SupplierId, request.WarehouseId, request.OrderDate,
            request.ExpectedDate, request.Notes, request.Currency, items), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<PurchaseOrderResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Update(
        Guid id, [FromBody] UpdatePurchaseOrderRequest request, CancellationToken ct)
    {
        var items = request.Items?.Select(i => (i.ProductId, i.Quantity, i.UnitCost))
            ?? Enumerable.Empty<(Guid, decimal, decimal?)>();
        var result = await _mediator.Send(new UpdatePurchaseOrderCommand(
            id, request.SupplierId, request.WarehouseId, request.OrderDate,
            request.ExpectedDate, request.Notes, request.Currency, items), ct);
        return Ok(ApiResponse<PurchaseOrderResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/submit")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Submit(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new SubmitPurchaseOrderCommand(id), ct);
        return Ok(ApiResponse<PurchaseOrderResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/receive")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponse>>> Receive(
        Guid id, [FromBody] ReceivePurchaseOrderRequest request, CancellationToken ct)
    {
        var items = request.Items.Select(i => (i.ProductId, i.ReceivedQuantity));
        var result = await _mediator.Send(new ReceivePurchaseOrderCommand(id, items, request.Notes), ct);
        return Ok(ApiResponse<PurchaseOrderResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/cancel")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new CancelPurchaseOrderCommand(id), ct);
        return NoContent();
    }
}
