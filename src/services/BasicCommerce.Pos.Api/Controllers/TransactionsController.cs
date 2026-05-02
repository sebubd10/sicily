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

    /// <summary>Opens a new POS transaction for the current terminal.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> Create(
        [FromBody] CreateTransactionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateTransactionCommand(
            _currentUser.TenantId,
            request.StoreId,
            request.TerminalId,
            _currentUser.UserId,
            request.CustomerId), ct);

        return CreatedAtAction(nameof(GetById),
            new { id = result.Id },
            ApiResponse<TransactionResponse>.Ok(result));
    }

    /// <summary>Gets a transaction by ID with all line items and payments.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetTransactionQuery(_currentUser.TenantId, id), ct);
        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    /// <summary>Adds a scanned or manually entered item to an open transaction.</summary>
    [HttpPost("{id:guid}/items")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> AddItem(
        Guid id, [FromBody] AddLineItemRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddLineItemCommand(
            _currentUser.TenantId,
            id,
            request.ProductId,
            request.Quantity,
            request.OverridePrice,
            request.OverrideApprovedBy), ct);

        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    /// <summary>Voids a single line item on an open transaction.</summary>
    [HttpDelete("{id:guid}/items/{lineItemId:guid}")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> VoidItem(
        Guid id, Guid lineItemId, CancellationToken ct)
    {
        var result = await _mediator.Send(new VoidLineItemCommand(
            _currentUser.TenantId,
            id,
            lineItemId,
            _currentUser.UserId), ct);

        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    /// <summary>Voids an entire transaction. Requires supervisor role.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> VoidTransaction(
        Guid id, [FromBody] VoidTransactionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new VoidTransactionCommand(
            _currentUser.TenantId,
            id,
            request.SupervisorId,
            request.Reason), ct);

        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }

    /// <summary>Completes the transaction with a payment.</summary>
    [HttpPost("{id:guid}/payment")]
    public async Task<ActionResult<ApiResponse<TransactionResponse>>> Pay(
        Guid id, [FromBody] AddPaymentRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<PaymentMethod>(request.Method, ignoreCase: true, out var method))
            return BadRequest(ApiResponse<TransactionResponse>.Fail(
                $"Unknown payment method '{request.Method}'."));

        var result = await _mediator.Send(new CompleteTransactionCommand(
            _currentUser.TenantId,
            id,
            method,
            request.Amount,
            request.MobileNumber,
            request.Reference), ct);

        return Ok(ApiResponse<TransactionResponse>.Ok(result));
    }
}

public record CreateTransactionRequest(
    Guid StoreId,
    Guid TerminalId,
    Guid? CustomerId = null);
