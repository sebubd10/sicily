using BasicCommerce.Application.Features.Promotions.Commands;
using BasicCommerce.Application.Features.Promotions.Queries;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Promotions;
using BasicCommerce.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/promotions")]
[Authorize(Policy = "StoreManagerAndAbove")]
public class PromotionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PromotionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PromotionListResponse>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var statusEnum = Enum.TryParse<PromotionStatus>(status, true, out var s)
            ? s : (PromotionStatus?)null;
        var result = await _mediator.Send(new GetPromotionsQuery(statusEnum, page, pageSize), ct);
        return Ok(ApiResponse<PromotionListResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PromotionResponse>>> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPromotionQuery(id), ct);
        return Ok(ApiResponse<PromotionResponse>.Ok(result));
    }

    [HttpGet("validate-coupon/{code}")]
    [Authorize(Policy = "AllAuthenticated")]
    public async Task<ActionResult<ApiResponse<PromotionResponse>>> ValidateCoupon(
        string code, CancellationToken ct)
    {
        var result = await _mediator.Send(new ValidateCouponQuery(code), ct);
        return Ok(ApiResponse<PromotionResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PromotionResponse>>> Create(
        [FromBody] CreatePromotionRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<PromotionType>(request.Type, true, out var type))
            return BadRequest(ApiResponse<PromotionResponse>.Fail(
                $"Unknown promotion type '{request.Type}'."));

        var result = await _mediator.Send(new CreatePromotionCommand(
            request.Name, request.Description, type,
            request.ProductId, request.CategoryId, request.StoreId,
            request.DiscountPercentage, request.DiscountAmount,
            request.BuyQuantity, request.GetQuantity,
            request.MinimumCartValue, request.CouponCode, request.RequiresCoupon,
            request.StartsAt, request.EndsAt, request.MaxUses), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<PromotionResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<ApiResponse<PromotionResponse>>> Activate(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ActivatePromotionCommand(id), ct);
        return Ok(ApiResponse<PromotionResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/pause")]
    public async Task<ActionResult<ApiResponse<PromotionResponse>>> Pause(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new PausePromotionCommand(id), ct);
        return Ok(ApiResponse<PromotionResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<PromotionResponse>>> Cancel(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelPromotionCommand(id), ct);
        return Ok(ApiResponse<PromotionResponse>.Ok(result));
    }
}
