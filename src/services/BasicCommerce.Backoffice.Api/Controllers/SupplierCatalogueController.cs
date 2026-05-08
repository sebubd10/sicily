using BasicCommerce.Application.Features.SupplierCatalogue;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Suppliers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/suppliers/{supplierId:guid}/catalogue")]
[Authorize(Policy = "StoreManagerAndAbove")]
public class SupplierCatalogueController : ControllerBase
{
    private readonly IMediator _mediator;

    public SupplierCatalogueController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns the paginated price catalogue for a supplier.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<SupplierProductListResponse>>> GetCatalogue(
        Guid supplierId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetSupplierCatalogueQuery(supplierId, page, pageSize), ct);
        return Ok(ApiResponse<SupplierProductListResponse>.Ok(result));
    }

    /// <summary>Looks up the catalogue price for a specific product from this supplier.</summary>
    [HttpGet("{productId:guid}")]
    public async Task<ActionResult<ApiResponse<SupplierProductResponse?>>> LookupPrice(
        Guid supplierId, Guid productId, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new LookupSupplierPriceQuery(supplierId, productId), ct);
        return Ok(ApiResponse<SupplierProductResponse?>.Ok(result));
    }

    /// <summary>Creates or updates a product price in the supplier's catalogue.</summary>
    [HttpPut("{productId:guid}")]
    public async Task<ActionResult<ApiResponse<SupplierProductResponse>>> Upsert(
        Guid supplierId, Guid productId,
        [FromBody] UpsertSupplierProductRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpsertSupplierProductCommand(
            supplierId, productId, request.UnitCost,
            request.CurrencyCode, request.SupplierSku,
            request.MinOrderQuantity, request.LeadTimeDays, request.Notes), ct);
        return Ok(ApiResponse<SupplierProductResponse>.Ok(result));
    }

    /// <summary>Removes a product from the supplier's catalogue.</summary>
    [HttpDelete("{productId:guid}")]
    [Authorize(Policy = "ChainAdminOnly")]
    public async Task<IActionResult> Delete(
        Guid supplierId, Guid productId, CancellationToken ct)
    {
        await _mediator.Send(new DeleteSupplierProductCommand(supplierId, productId), ct);
        return NoContent();
    }
}

// ── Separate route: which suppliers carry this product? ───────
[ApiController]
[Route("api/products/{productId:guid}/suppliers")]
[Authorize(Policy = "StoreManagerAndAbove")]
public class ProductSuppliersController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductSuppliersController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Returns all suppliers that carry this product, ordered cheapest first.
    /// Useful for selecting the best supplier when creating a purchase order.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SupplierProductResponse>>>> GetSuppliers(
        Guid productId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductSuppliersQuery(productId), ct);
        return Ok(ApiResponse<IReadOnlyList<SupplierProductResponse>>.Ok(result));
    }
}
