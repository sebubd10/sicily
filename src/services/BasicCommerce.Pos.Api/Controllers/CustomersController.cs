using BasicCommerce.Application.Features.Customers.Commands;
using BasicCommerce.Application.Features.Customers.Queries;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Customers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Pos.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Quick lookup by code, phone, or name — used at POS to attach a customer.</summary>
    [HttpGet("lookup")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomerResponse>>>> Lookup(
        [FromQuery] string term, CancellationToken ct)
    {
        var result = await _mediator.Send(new SearchCustomersQuery(term, 1, 10), ct);
        return Ok(ApiResponse<IEnumerable<CustomerResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCustomerQuery(id), ct);
        return Ok(ApiResponse<CustomerResponse>.Ok(result));
    }

    /// <summary>Register a walk-in customer at the POS terminal.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> Register(
        [FromBody] RegisterCustomerRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RegisterCustomerCommand(
            request.Name, request.Email, request.Phone, 0,
            request.AddressLine1, request.City, request.District, request.PostalCode), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<CustomerResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/loyalty/redeem")]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> RedeemLoyaltyPoints(
        Guid id, [FromBody] AdjustLoyaltyPointsRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RedeemLoyaltyPointsCommand(id, request.Points), ct);
        return Ok(ApiResponse<CustomerResponse>.Ok(result));
    }
}
