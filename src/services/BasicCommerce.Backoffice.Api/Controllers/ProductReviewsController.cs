using BasicCommerce.Application.Features.ProductReviews.Commands;
using BasicCommerce.Application.Features.ProductReviews.Queries;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.ProductReviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/product-reviews")]
[Authorize]
public class ProductReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductReviewsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ProductReviewListResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? storeId = null,
        [FromQuery] bool? isApproved = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetProductReviewsQuery(page, pageSize, productId, storeId, isApproved, search), ct);
        return Ok(ApiResponse<ProductReviewListResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductReviewResponse>>> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductReviewQuery(id), ct);
        return Ok(ApiResponse<ProductReviewResponse>.Ok(result));
    }

    [HttpGet("products/{productId:guid}/rating")]
    public async Task<ActionResult<ApiResponse<ProductRatingSummaryResponse>>> GetRating(
        Guid productId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductRatingSummaryQuery(productId), ct);
        return Ok(ApiResponse<ProductRatingSummaryResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<ProductReviewResponse>>> Approve(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApproveProductReviewCommand(id), ct);
        return Ok(ApiResponse<ProductReviewResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<ProductReviewResponse>>> Reject(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new RejectProductReviewCommand(id), ct);
        return Ok(ApiResponse<ProductReviewResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteProductReviewCommand(id), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/replies")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<ProductReviewResponse>>> AddAdminReply(
        Guid id, [FromBody] AddReviewDetailRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new AddReviewDetailCommand(id, request.Comment, IsAdminReply: true), ct);
        return Ok(ApiResponse<ProductReviewResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}/replies/{detailId:guid}")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> DeleteReply(Guid id, Guid detailId, CancellationToken ct)
    {
        await _mediator.Send(new DeleteReviewDetailCommand(id, detailId), ct);
        return NoContent();
    }
}
