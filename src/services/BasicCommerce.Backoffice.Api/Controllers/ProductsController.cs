using BasicCommerce.Application.Features.ProductImages.Commands;
using BasicCommerce.Application.Features.ProductImages.Queries;
using BasicCommerce.Application.Features.Products.Commands;
using BasicCommerce.Application.Features.Products.Queries;
using BasicCommerce.Application.Features.ProductTags.Commands;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public ProductsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ProductListResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] EntityStatus? status = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetProductsQuery(page, pageSize, categoryId, status, search), ct);
        return Ok(ApiResponse<ProductListResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductQuery(id), ct);
        return Ok(ApiResponse<ProductResponse>.Ok(result));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductResponse>>>> Search(
        [FromQuery] string term, [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new SearchProductsQuery(_currentUser.TenantId, term, limit), ct);
        return Ok(ApiResponse<IEnumerable<ProductResponse>>.Ok(result));
    }

    [HttpGet("barcode/{barcode}")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> GetByBarcode(
        string barcode, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetProductByBarcodeQuery(_currentUser.TenantId, barcode), ct);
        return Ok(ApiResponse<ProductResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> Create(
        [FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateProductCommand(
            request.Sku,
            request.Barcode,
            request.Plu,
            request.Name,
            request.NameBn,
            request.CategoryId,
            request.Price,
            request.VatRateId,
            request.UnitType,
            request.IsWeightBased,
            request.IsPerishable,
            request.IsAgeRestricted,
            request.AgeRestrictionYears,
            request.CostPrice,
            request.Description,
            request.UnitLabel,
            request.ManufacturerId,
            request.TagIds,
            request.IsEbtEligible,
            request.TrackInventory,
            request.ReorderLevel), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<ProductResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> Update(
        Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateProductCommand(
            id,
            request.Name,
            request.NameBn,
            request.Description,
            request.CategoryId,
            request.Price,
            request.VatRateId,
            request.UnitType,
            request.UnitLabel,
            request.IsWeightBased,
            request.IsPerishable,
            request.IsAgeRestricted,
            request.AgeRestrictionYears,
            request.IsEbtEligible,
            request.TrackInventory,
            request.ReorderLevel,
            request.ImageUrl,
            request.CostPrice,
            request.ManufacturerId,
            request.TagIds,
            request.Plu), ct);
        return Ok(ApiResponse<ProductResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/price")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> UpdatePrice(
        Guid id, [FromBody] UpdateProductPriceRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateProductPriceCommand(id, request.NewPrice), ct);
        return Ok(ApiResponse<ProductResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateProductCommand(id), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/activate")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ActivateProductCommand(id), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/tags")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> SetTags(
        Guid id, [FromBody] SetProductTagsRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SetProductTagsCommand(id, request.TagIds), ct);
        return Ok(ApiResponse<ProductResponse>.Ok(result));
    }

    // ── Product Images ───────────────────────────────────────────────────────

    [HttpGet("{id:guid}/images")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductImageResponse>>>> GetImages(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductImagesQuery(id), ct);
        return Ok(ApiResponse<IEnumerable<ProductImageResponse>>.Ok(result));
    }

    [HttpPost("{id:guid}/images/url")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<ProductImageResponse>>> AddImageByUrl(
        Guid id, [FromBody] AddProductImageByUrlRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddProductImageByUrlCommand(
            id, request.Title, request.Url, request.Description, request.SortOrder), ct);
        return Ok(ApiResponse<ProductImageResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/images/upload")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<ProductImageResponse>>> AddImageByUpload(
        Guid id, IFormFile file,
        [FromForm] string title,
        [FromForm] string? description = null,
        [FromForm] int sortOrder = 0,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new AddProductImageByUploadCommand(
            id, title, file.OpenReadStream(), file.FileName, description, sortOrder), ct);
        return Ok(ApiResponse<ProductImageResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/images/{imageId:guid}")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<ProductImageResponse>>> UpdateImage(
        Guid id, Guid imageId, [FromBody] UpdateProductImageRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateProductImageCommand(imageId, request.Title, request.Description), ct);
        return Ok(ApiResponse<ProductImageResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId, CancellationToken ct)
    {
        await _mediator.Send(new DeleteProductImageCommand(imageId), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/images/reorder")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<IActionResult> ReorderImages(
        Guid id, [FromBody] ReorderProductImagesRequest request, CancellationToken ct)
    {
        var items = request.Items.Select(i => (i.Id, i.SortOrder));
        await _mediator.Send(new ReorderProductImagesCommand(id, items), ct);
        return NoContent();
    }
}
