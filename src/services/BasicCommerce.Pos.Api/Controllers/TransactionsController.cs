using BasicCommerce.Application.Features.Transactions.Commands;
using BasicCommerce.Application.Features.Transactions.Queries;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Transactions;
using BasicCommerce.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Pos.Api.Controllers;

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
        [FromBody] CreateTransactionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateTransactionCommand(
            _currentUser.TenantId, request.StoreId, request.TerminalId,
            _currentUser.UserId, request.CustomerId), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<TransactionResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetTransactionQuery(_currentUser.TenantId, id), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    // ── Items ─────────────────────────────────────────────────────────────────

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

    [HttpPost("{id:guid}/items/{lineItemId:guid}/discount")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> ApplyDiscount(
        Guid id, Guid lineItemId, [FromBody] ApplyDiscountRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApplyLineItemDiscountCommand(
            _currentUser.TenantId, id, lineItemId, request.DiscountAmount), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    // ── Customer ──────────────────────────────────────────────────────────────

    [HttpPut("{id:guid}/customer")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> AttachCustomer(
        Guid id, [FromBody] AttachCustomerRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AttachCustomerCommand(
            _currentUser.TenantId, id, request.CustomerId), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    // ── Payments ──────────────────────────────────────────────────────────────

    /// <summary>Add a single payment (call multiple times for split payment).</summary>
    [HttpPost("{id:guid}/payments")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> AddPayment(
        Guid id, [FromBody] AddPaymentRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<PaymentMethod>(request.Method, ignoreCase: true, out var method))
            return BadRequest(ApiResponse<TransactionResponse>.Fail(
                $"Unknown payment method '{request.Method}'."));

        var result = await _mediator.Send(new AddPaymentCommand(
            _currentUser.TenantId, id, method, request.Amount,
            request.MobileNumber, request.Reference), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    /// <summary>Explicitly complete after all payments have been added.</summary>
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> Complete(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CompleteTransactionCommand(_currentUser.TenantId, id), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    /// <summary>Convenience: add a single payment and complete in one call.</summary>
    [HttpPost("{id:guid}/payment")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> Pay(
        Guid id, [FromBody] AddPaymentRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<PaymentMethod>(request.Method, ignoreCase: true, out var method))
            return BadRequest(ApiResponse<TransactionResponse>.Fail(
                $"Unknown payment method '{request.Method}'."));

        await _mediator.Send(new AddPaymentCommand(
            _currentUser.TenantId, id, method, request.Amount,
            request.MobileNumber, request.Reference), ct);

        var result = await _mediator.Send(
            new CompleteTransactionCommand(_currentUser.TenantId, id), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/suspend")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> Suspend(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new SuspendTransactionCommand(_currentUser.TenantId, id), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/recall")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> Recall(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new RecallTransactionCommand(_currentUser.TenantId, id), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> VoidTransaction(
        Guid id, [FromBody] VoidTransactionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new VoidTransactionCommand(
            _currentUser.TenantId, id, request.SupervisorId, request.Reason), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    // ── Returns ───────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/return")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> CreateReturn(
        Guid id, [FromBody] CreateReturnTransactionRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<PaymentMethod>(request.RefundMethod, ignoreCase: true, out var method))
            return BadRequest(ApiResponse<TransactionResponse>.Fail(
                $"Unknown refund method '{request.RefundMethod}'."));

        var result = await _mediator.Send(new CreateReturnTransactionCommand(
            _currentUser.TenantId,
            id,
            GetTerminalIdFromContext(),
            request.Items.Select(i => new ReturnItem(
                i.OriginalLineItemId,
                i.Quantity,
                Enum.TryParse<ReturnReason>(i.ReturnReason, true, out var reason)
                    ? reason : ReturnReason.Other,
                Enum.TryParse<DamageDisposition>(i.DamageDisposition, true, out var disp)
                    ? disp : DamageDisposition.RestoreToStock)),
            method,
            request.Notes), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    private Guid GetTerminalIdFromContext()
    {
        var claim = User.FindFirst("terminal_id")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}

public record CreateTransactionRequest(
    Guid StoreId,
    Guid TerminalId,
    Guid? CustomerId = null);
