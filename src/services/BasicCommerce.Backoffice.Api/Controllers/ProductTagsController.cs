using BasicCommerce.Application.Features.ProductTags.Commands;
using BasicCommerce.Application.Features.ProductTags.Queries;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.ProductTags;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/product-tags")]
[Authorize]
public class ProductTagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductTagsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ProductTagListResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetProductTagsQuery(page, pageSize, search), ct);
        return Ok(ApiResponse<ProductTagListResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductTagDetailResponse>>> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductTagQuery(id), ct);
        return Ok(ApiResponse<ProductTagDetailResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<ProductTagDetailResponse>>> Create(
        [FromBody] CreateProductTagRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateProductTagCommand(request.Name), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ProductTagDetailResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<ProductTagDetailResponse>>> Update(
        Guid id, [FromBody] UpdateProductTagRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateProductTagCommand(id, request.Name), ct);
        return Ok(ApiResponse<ProductTagDetailResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteProductTagCommand(id), ct);
        return NoContent();
    }

    [HttpPost("bulk-delete")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> BulkDelete(
        [FromBody] BulkDeleteProductTagsRequest request, CancellationToken ct)
    {
        await _mediator.Send(new BulkDeleteProductTagsCommand(request.Ids), ct);
        return NoContent();
    }
}
