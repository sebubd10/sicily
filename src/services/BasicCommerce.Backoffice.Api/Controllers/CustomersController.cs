using BasicCommerce.Application.Features.Customers.Commands;
using BasicCommerce.Application.Features.Customers.Queries;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Customers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public CustomersController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CustomerListResponse>>> Search(
        [FromQuery] string? term, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new SearchCustomersQuery(term, page, pageSize), ct);
        return Ok(ApiResponse<CustomerListResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> GetById(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCustomerQuery(id), ct);
        return Ok(ApiResponse<CustomerResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> Register(
        [FromBody] RegisterCustomerRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RegisterCustomerCommand(
            request.Name, request.Email, request.Phone, request.CreditLimit,
            request.AddressLine1, request.City, request.District, request.PostalCode), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<CustomerResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> Update(
        Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateCustomerCommand(
            id, request.Name, request.Email, request.Phone,
            request.AddressLine1, request.City, request.District, request.PostalCode), ct);
        return Ok(ApiResponse<CustomerResponse>.Ok(result));
    }

    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateCustomerCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(null!));
    }

    [HttpPut("{id:guid}/activate")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<object>>> Activate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ActivateCustomerCommand(id), ct);
        return Ok(ApiResponse<object>.Ok(null!));
    }

    [HttpPut("{id:guid}/credit-limit")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> UpdateCreditLimit(
        Guid id, [FromBody] UpdateCreditLimitRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateCreditLimitCommand(id, request.CreditLimit), ct);
        return Ok(ApiResponse<CustomerResponse>.Ok(result));
    }

    [HttpPost("credit-accounts")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<CreditAccountSummary>>> CreateCreditAccount(
        [FromBody] CreateCreditAccountRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateCreditAccountCommand(request.CustomerId, request.StoreId, request.CreditLimit), ct);
        return Ok(ApiResponse<CreditAccountSummary>.Ok(result));
    }

    [HttpGet("credit-accounts")]
    public async Task<ActionResult<ApiResponse<CreditAccountListResponse>>> GetAllCreditAccounts(
        [FromQuery] string? term, [FromQuery] bool? hasBalance,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllCreditAccountsQuery(term, hasBalance, page, pageSize), ct);
        return Ok(ApiResponse<CreditAccountListResponse>.Ok(result));
    }

    [HttpGet("{id:guid}/credit-accounts")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CreditAccountResponse>>>> GetCreditAccounts(
        Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCustomerCreditAccountsQuery(id), ct);
        return Ok(ApiResponse<IEnumerable<CreditAccountResponse>>.Ok(result));
    }

    [HttpGet("credit-accounts/{creditAccountId:guid}")]
    public async Task<ActionResult<ApiResponse<CreditAccountResponse>>> GetCreditAccount(
        Guid creditAccountId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCreditAccountQuery(creditAccountId), ct);
        return Ok(ApiResponse<CreditAccountResponse>.Ok(result));
    }

    [HttpPost("credit-accounts/{creditAccountId:guid}/payments")]
    public async Task<ActionResult<ApiResponse<CreditAccountResponse>>> RecordPayment(
        Guid creditAccountId, [FromBody] RecordCreditPaymentRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new RecordCreditPaymentCommand(creditAccountId, request.Amount, request.Reference), ct);
        return Ok(ApiResponse<CreditAccountResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/loyalty/add")]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> AddLoyaltyPoints(
        Guid id, [FromBody] AdjustLoyaltyPointsRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddLoyaltyPointsCommand(id, request.Points), ct);
        return Ok(ApiResponse<CustomerResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/loyalty/redeem")]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> RedeemLoyaltyPoints(
        Guid id, [FromBody] AdjustLoyaltyPointsRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RedeemLoyaltyPointsCommand(id, request.Points), ct);
        return Ok(ApiResponse<CustomerResponse>.Ok(result));
    }
}
