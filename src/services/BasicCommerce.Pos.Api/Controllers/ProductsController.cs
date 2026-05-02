using BasicCommerce.Application.Features.Products.Queries;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Pos.Api.Controllers;

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

    /// <summary>Looks up a product by barcode — called on every scan.</summary>
    [HttpGet("barcode/{barcode}")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> GetByBarcode(
        string barcode, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetProductByBarcodeQuery(_currentUser.TenantId, barcode), ct);
        return Ok(ApiResponse<ProductResponse>.Ok(result));
    }

    /// <summary>Looks up a product by PLU code — used for weighted items.</summary>
    [HttpGet("plu/{plu}")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> GetByPlu(
        string plu, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetProductByPluQuery(_currentUser.TenantId, plu), ct);
        return Ok(ApiResponse<ProductResponse>.Ok(result));
    }

    /// <summary>Full-text product search — used for manual item lookup.</summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductResponse>>>> Search(
        [FromQuery] string term, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new SearchProductsQuery(_currentUser.TenantId, term), ct);
        return Ok(ApiResponse<IEnumerable<ProductResponse>>.Ok(result));
    }
}
