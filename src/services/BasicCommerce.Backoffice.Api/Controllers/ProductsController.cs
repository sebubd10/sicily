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

    // POST /api/products — create product (wired to CreateProductCommand, to be built)
    [HttpPost]
    public IActionResult Create([FromBody] CreateProductRequest request) =>
        StatusCode(501, ApiResponse<object>.Fail("CreateProductCommand not yet implemented."));

    // PUT /api/products/{id}/price — update price
    [HttpPut("{id:guid}/price")]
    public IActionResult UpdatePrice(Guid id, [FromBody] UpdateProductPriceRequest request) =>
        StatusCode(501, ApiResponse<object>.Fail("UpdateProductPriceCommand not yet implemented."));
}
