using BasicCommerce.Application.Features.Products.Commands;
using BasicCommerce.Application.Features.Products.Queries;
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
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetProductsQuery(page, pageSize, categoryId, status), ct);
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
            request.IsAgeRestricted,
            request.AgeRestrictionYears,
            request.CostPrice,
            request.Description,
            request.UnitLabel), ct);
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
            request.VatRateId,
            request.UnitType,
            request.UnitLabel,
            request.IsWeightBased,
            request.IsAgeRestricted,
            request.AgeRestrictionYears,
            request.IsEbtEligible,
            request.TrackInventory,
            request.ReorderLevel,
            request.ImageUrl,
            request.CostPrice), ct);
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
}
