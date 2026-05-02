using BasicCommerce.Application.Features.Products.Commands;
using BasicCommerce.Application.Features.Products.Queries;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Products;
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
            request.CostPrice), ct);
        return CreatedAtAction(nameof(GetByBarcode), new { barcode = result.Barcode },
            ApiResponse<ProductResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/price")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> UpdatePrice(
        Guid id, [FromBody] UpdateProductPriceRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateProductPriceCommand(id, request.NewPrice), ct);
        return Ok(ApiResponse<ProductResponse>.Ok(result));
    }
}
