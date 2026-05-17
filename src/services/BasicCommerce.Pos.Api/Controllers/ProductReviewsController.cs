using BasicCommerce.Application.Features.ProductReviews.Commands;
using BasicCommerce.Application.Features.ProductReviews.Queries;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.ProductReviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Pos.Api.Controllers;

[ApiController]
[Route("api/product-reviews")]
[Authorize]
public class ProductReviewsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public ProductReviewsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet("products/{productId:guid}")]
    public async Task<ActionResult<ApiResponse<ProductReviewListResponse>>> GetByProduct(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetProductReviewsByProductQuery(productId, page, pageSize, ApprovedOnly: true), ct);
        return Ok(ApiResponse<ProductReviewListResponse>.Ok(result));
    }

    [HttpGet("products/{productId:guid}/rating")]
    public async Task<ActionResult<ApiResponse<ProductRatingSummaryResponse>>> GetRating(
        Guid productId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductRatingSummaryQuery(productId), ct);
        return Ok(ApiResponse<ProductRatingSummaryResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductReviewResponse>>> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductReviewQuery(id), ct);
        return Ok(ApiResponse<ProductReviewResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductReviewResponse>>> Submit(
        [FromBody] SubmitProductReviewRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SubmitProductReviewCommand(
            request.ProductId, null, request.StoreId,
            request.CustomerName, request.Title,
            request.ReviewText, request.Rating), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ProductReviewResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/replies")]
    public async Task<ActionResult<ApiResponse<ProductReviewResponse>>> AddReply(
        Guid id, [FromBody] AddReviewDetailRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new AddReviewDetailCommand(id, request.Comment, IsAdminReply: false), ct);
        return Ok(ApiResponse<ProductReviewResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/helpful")]
    public async Task<IActionResult> MarkHelpful(
        Guid id, [FromBody] MarkReviewHelpfulRequest request,
        [FromQuery] Guid customerId, CancellationToken ct)
    {
        await _mediator.Send(new MarkReviewHelpfulCommand(id, customerId, request.IsHelpful), ct);
        return NoContent();
    }
}
