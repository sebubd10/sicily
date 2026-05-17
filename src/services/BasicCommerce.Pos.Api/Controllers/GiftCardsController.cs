using BasicCommerce.Application.Features.GiftCards.Queries;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.GiftCards;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Pos.Api.Controllers;

[ApiController]
[Route("api/gift-cards")]
[Authorize]
public class GiftCardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GiftCardsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("check/{code}")]
    public async Task<ActionResult<ApiResponse<CheckBalanceResponse>>> CheckBalance(
        string code, CancellationToken ct)
    {
        var result = await _mediator.Send(new CheckGiftCardBalanceQuery(code), ct);
        return Ok(ApiResponse<CheckBalanceResponse>.Ok(result));
    }
}
