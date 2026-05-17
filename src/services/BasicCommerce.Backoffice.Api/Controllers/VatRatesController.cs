using BasicCommerce.Application.Features.Products.Queries;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/vat-rates")]
[Authorize]
public class VatRatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VatRatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<VatRateResponse>>>> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVatRatesQuery(), ct);
        return Ok(ApiResponse<IEnumerable<VatRateResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<VatRateResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVatRateQuery(id), ct);
        return Ok(ApiResponse<VatRateResponse>.Ok(result));
    }
}
