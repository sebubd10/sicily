using BasicCommerce.Application.Features.Inventory.Commands;
using BasicCommerce.Application.Features.Inventory.Queries;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Inventory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize(Policy = "StoreManagerAndAbove")]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ── Stock Levels ──────────────────────────────────────────────────────────

    [HttpGet("{storeId:guid}/stock")]
    public async Task<ActionResult<ApiResponse<IEnumerable<StockLevelResponse>>>> GetStock(
        Guid storeId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStockLevelsQuery(storeId), ct);
        return Ok(ApiResponse<IEnumerable<StockLevelResponse>>.Ok(result));
    }

    [HttpGet("{storeId:guid}/stock/{productId:guid}")]
    public async Task<ActionResult<ApiResponse<StockLevelResponse>>> GetStockLevel(
        Guid storeId, Guid productId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStockLevelQuery(storeId, productId), ct);
        return Ok(ApiResponse<StockLevelResponse>.Ok(result));
    }

    [HttpGet("{storeId:guid}/low-stock")]
    public async Task<ActionResult<ApiResponse<IEnumerable<StockLevelResponse>>>> GetLowStock(
        Guid storeId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLowStockAlertsQuery(storeId), ct);
        return Ok(ApiResponse<IEnumerable<StockLevelResponse>>.Ok(result));
    }

    // ── Stock Movements (audit log) ───────────────────────────────────────────

    [HttpGet("{storeId:guid}/movements")]
    public async Task<ActionResult<ApiResponse<IEnumerable<StockMovementResponse>>>> GetMovements(
        Guid storeId,
        [FromQuery] Guid? productId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetStockMovementsQuery(storeId, productId, from, to, limit), ct);
        return Ok(ApiResponse<IEnumerable<StockMovementResponse>>.Ok(result));
    }

    // ── Mutating Operations ───────────────────────────────────────────────────

    [HttpPost("{storeId:guid}/receive")]
    public async Task<ActionResult<ApiResponse<StockLevelResponse>>> Receive(
        Guid storeId, [FromBody] ReceiveStockRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ReceiveStockCommand(
            storeId, request.ProductId, request.Quantity, request.Reference, request.Notes), ct);
        return Ok(ApiResponse<StockLevelResponse>.Ok(result));
    }

    [HttpPost("{storeId:guid}/adjust")]
    public async Task<ActionResult<ApiResponse<StockLevelResponse>>> Adjust(
        Guid storeId, [FromBody] AdjustStockRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AdjustStockCommand(
            storeId, request.ProductId, request.NewQuantity, request.Notes), ct);
        return Ok(ApiResponse<StockLevelResponse>.Ok(result));
    }

    [HttpPost("{storeId:guid}/write-off")]
    public async Task<ActionResult<ApiResponse<StockLevelResponse>>> WriteOff(
        Guid storeId, [FromBody] WriteOffStockRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new WriteOffStockCommand(
            storeId, request.ProductId, request.Quantity, request.Reason), ct);
        return Ok(ApiResponse<StockLevelResponse>.Ok(result));
    }

    [HttpPost("transfer")]
    public async Task<ActionResult<ApiResponse<StockLevelResponse>>> Transfer(
        [FromBody] TransferStockRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new TransferStockCommand(
            request.SourceStoreId, request.DestinationStoreId,
            request.ProductId, request.Quantity, request.Notes), ct);
        return Ok(ApiResponse<StockLevelResponse>.Ok(result));
    }

    [HttpPut("{storeId:guid}/threshold")]
    public async Task<ActionResult<ApiResponse<StockLevelResponse>>> SetThreshold(
        Guid storeId, [FromBody] SetLowStockThresholdRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SetLowStockThresholdCommand(
            storeId, request.ProductId, request.Threshold), ct);
        return Ok(ApiResponse<StockLevelResponse>.Ok(result));
    }

    // ── Stock Batches (perishable / expiry tracking) ─────────────────────────

    [HttpPost("{storeId:guid}/batches")]
    public async Task<ActionResult<ApiResponse<StockBatchResponse>>> ReceiveBatch(
        Guid storeId, [FromBody] ReceiveStockBatchRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ReceiveStockBatchCommand(
            storeId, request.ProductId, request.Quantity, request.ExpiryDate,
            request.LotNumber, request.UnitCost, request.PurchaseOrderId,
            request.Reference, request.Notes), ct);
        return Ok(ApiResponse<StockBatchResponse>.Ok(result));
    }

    [HttpGet("{storeId:guid}/batches")]
    public async Task<ActionResult<ApiResponse<StockBatchListResponse>>> GetBatches(
        Guid storeId,
        [FromQuery] Guid? productId,
        [FromQuery] bool includeExpired = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetStockBatchesQuery(storeId, productId, includeExpired, page, pageSize), ct);
        return Ok(ApiResponse<StockBatchListResponse>.Ok(result));
    }

    [HttpGet("{storeId:guid}/batches/expiring")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StockBatchResponse>>>> GetExpiringBatches(
        Guid storeId,
        [FromQuery] int withinDays = 30,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetExpiringBatchesQuery(storeId, withinDays), ct);
        return Ok(ApiResponse<IReadOnlyList<StockBatchResponse>>.Ok(result));
    }

    [HttpPost("{storeId:guid}/batches/expire")]
    public async Task<ActionResult<ApiResponse<int>>> ExpireBatches(
        Guid storeId, [FromBody] ExpireStockBatchesRequest request, CancellationToken ct)
    {
        var count = await _mediator.Send(new ExpireStockBatchesCommand(storeId, request.Notes), ct);
        return Ok(ApiResponse<int>.Ok(count));
    }
}
