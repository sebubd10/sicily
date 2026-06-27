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
}
