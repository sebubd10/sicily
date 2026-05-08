using BasicCommerce.Application.Features.GiftCards.Commands;
using BasicCommerce.Application.Features.GiftCards.Queries;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.GiftCards;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/gift-cards")]
[Authorize(Policy = "StoreManagerAndAbove")]
public class GiftCardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GiftCardsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<GiftCardListResponse>>> GetAll(
        [FromQuery] Guid? storeId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetGiftCardsQuery(storeId, status, page, pageSize), ct);
        return Ok(ApiResponse<GiftCardListResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<GiftCardResponse>>> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetGiftCardQuery(id), ct);
        return Ok(ApiResponse<GiftCardResponse>.Ok(result));
    }

    [HttpGet("check/{code}")]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<ActionResult<ApiResponse<CheckBalanceResponse>>> CheckBalance(
        string code, CancellationToken ct)
    {
        var result = await _mediator.Send(new CheckGiftCardBalanceQuery(code), ct);
        return Ok(ApiResponse<CheckBalanceResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<GiftCardResponse>>> Issue(
        [FromBody] IssueGiftCardRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new IssueGiftCardCommand(
            request.StoreId, request.Amount, request.ExpiryDate,
            request.IssuedToCustomerId, null, request.Notes), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<GiftCardResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/reload")]
    public async Task<ActionResult<ApiResponse<GiftCardResponse>>> Reload(
        Guid id, [FromBody] ReloadGiftCardRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ReloadGiftCardCommand(id, request.Amount, request.Notes), ct);
        return Ok(ApiResponse<GiftCardResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<GiftCardResponse>>> Cancel(
        Guid id, [FromBody] string? reason, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelGiftCardCommand(id, reason), ct);
        return Ok(ApiResponse<GiftCardResponse>.Ok(result));
    }
}
