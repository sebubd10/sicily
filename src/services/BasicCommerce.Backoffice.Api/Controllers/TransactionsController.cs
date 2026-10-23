using BasicCommerce.Application.Features.Transactions.Commands;
using BasicCommerce.Application.Features.Transactions.Queries;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public TransactionsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> Create(
        [FromBody] BackofficeCreateTransactionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateTransactionCommand(
            _currentUser.TenantId, request.StoreId, request.TerminalId,
            _currentUser.UserId, request.CustomerId), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<TransactionResponse>.Ok(result));
    }

    /// <summary>Price a cart (promotions, discount, VAT) without saving anything.</summary>
    [HttpPost("preview")]
    public async Task<ActionResult<ApiResponse<CartPreviewResponse>>> Preview(
        [FromBody] CartPreviewRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new PreviewCartQuery(
            _currentUser.TenantId, request.StoreId, request.Items.ToList()), ct);
        return Ok(ApiResponse<CartPreviewResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/items")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> AddItem(
        Guid id, [FromBody] AddLineItemRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddLineItemCommand(
            _currentUser.TenantId, id, request.ProductId, request.Quantity,
            request.OverridePrice, request.OverrideApprovedBy), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}/items/{lineItemId:guid}")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> VoidItem(
        Guid id, Guid lineItemId, CancellationToken ct)
    {
        var result = await _mediator.Send(new VoidLineItemCommand(
            _currentUser.TenantId, id, lineItemId, _currentUser.UserId), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/customer")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> AttachCustomer(
        Guid id, [FromBody] AttachCustomerRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AttachCustomerCommand(
            _currentUser.TenantId, id, request.CustomerId), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/payments")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> AddPayment(
        Guid id, [FromBody] AddPaymentRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<PaymentMethod>(request.Method, ignoreCase: true, out var method))
            return BadRequest(ApiResponse<TransactionResponse>.Fail(
                $"Unknown payment method '{request.Method}'."));

        var result = await _mediator.Send(new AddPaymentCommand(
            _currentUser.TenantId, id, method, request.Amount,
            request.MobileNumber, request.Reference, request.GiftCardCode), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> Complete(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CompleteTransactionCommand(_currentUser.TenantId, id), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<TransactionListResponse>>> Search(
        [FromQuery] string? term,
        [FromQuery] Guid? storeId,
        [FromQuery] string? status,
        [FromQuery] string? type,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new SearchTransactionsQuery(
            _currentUser.TenantId, term, storeId, status, type,
            from, to, customerId, page, pageSize), ct);
        return Ok(ApiResponse<TransactionListResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetTransactionQuery(_currentUser.TenantId, id), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/void")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> Void(
        Guid id, [FromBody] BackofficeVoidRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new VoidTransactionCommand(
            _currentUser.TenantId, id, _currentUser.UserId, request.Reason), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/return")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> CreateReturn(
        Guid id, [FromBody] BackofficeReturnRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<PaymentMethod>(request.RefundMethod, ignoreCase: true, out var method))
            return BadRequest(ApiResponse<TransactionResponse>.Fail(
                $"Unknown refund method '{request.RefundMethod}'."));

        var items = request.Items.Select(i => new ReturnItem(
            i.OriginalLineItemId,
            i.Quantity,
            Enum.TryParse<ReturnReason>(i.ReturnReason, true, out var reason)
                ? reason : ReturnReason.Other,
            Enum.TryParse<DamageDisposition>(i.DamageDisposition, true, out var disp)
                ? disp : DamageDisposition.RestoreToStock));

        var result = await _mediator.Send(new CreateReturnTransactionCommand(
            _currentUser.TenantId, id, null, items, method, request.Notes), ct);

        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    // ── Editing an unfinished sale ────────────────────────────────────────────

    [HttpPut("{id:guid}/items/{lineItemId:guid}/quantity")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> ChangeQuantity(
        Guid id, Guid lineItemId, [FromBody] ChangeQuantityRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ChangeLineItemQuantityCommand(
            _currentUser.TenantId, id, lineItemId, request.Quantity), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}/payments/{paymentId:guid}")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> RemovePayment(
        Guid id, Guid paymentId, CancellationToken ct)
    {
        var result = await _mediator.Send(new RemovePaymentCommand(_currentUser.TenantId, id, paymentId), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}/customer")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> DetachCustomer(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DetachCustomerCommand(_currentUser.TenantId, id), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/suspend")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> Suspend(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new SuspendTransactionCommand(_currentUser.TenantId, id), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/recall")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> Recall(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new RecallTransactionCommand(_currentUser.TenantId, id), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    // ── Completed sales: history, corrections, re-ring, exchange ──────────────

    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TransactionAuditEntryResponse>>>> History(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTransactionHistoryQuery(_currentUser.TenantId, id), ct);
        return Ok(ApiResponse<IReadOnlyList<TransactionAuditEntryResponse>>.Ok(result));
    }

    [HttpPost("{id:guid}/correct")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> Correct(
        Guid id, [FromBody] CorrectTransactionRequest request, CancellationToken ct)
    {
        var corrections = new List<PaymentMethodCorrection>();
        foreach (var c in request.PaymentCorrections ?? Enumerable.Empty<PaymentMethodCorrectionRequest>())
        {
            if (!Enum.TryParse<PaymentMethod>(c.NewMethod, ignoreCase: true, out var method))
                return BadRequest(ApiResponse<TransactionResponse>.Fail($"Unknown payment method '{c.NewMethod}'."));
            corrections.Add(new PaymentMethodCorrection(c.PaymentId, method, c.MobileNumber));
        }

        var result = await _mediator.Send(new CorrectTransactionCommand(
            _currentUser.TenantId, id, request.Reason, request.UpdateNotes, request.Notes,
            request.AttachCustomerId, corrections), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    /// <summary>Voids the sale and returns a new draft with the same customer and items.</summary>
    [HttpPost("{id:guid}/void-and-rering")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> VoidAndReRing(
        Guid id, [FromBody] VoidAndReRingRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new VoidAndReRingCommand(
            _currentUser.TenantId, id, _currentUser.UserId, request.Reason), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/exchange")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<ExchangeResponse>>> Exchange(
        Guid id, [FromBody] ExchangeRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<PaymentMethod>(request.SettlementMethod, ignoreCase: true, out var method))
            return BadRequest(ApiResponse<ExchangeResponse>.Fail(
                $"Unknown settlement method '{request.SettlementMethod}'."));

        var returnItems = request.ReturnItems.Select(i => new ReturnItem(
            i.OriginalLineItemId,
            i.Quantity,
            Enum.TryParse<ReturnReason>(i.ReturnReason, true, out var reason) ? reason : ReturnReason.Other,
            Enum.TryParse<DamageDisposition>(i.DamageDisposition, true, out var disp)
                ? disp : DamageDisposition.RestoreToStock)).ToList();

        var newItems = request.NewItems.Select(i => new ExchangeNewItem(i.ProductId, i.Quantity)).ToList();

        var result = await _mediator.Send(new ExchangeTransactionCommand(
            _currentUser.TenantId, id, returnItems, newItems, method,
            request.SettlementMobileNumber, request.Notes), ct);
        return Ok(ApiResponse<ExchangeResponse>.Ok(result));
    }
}
