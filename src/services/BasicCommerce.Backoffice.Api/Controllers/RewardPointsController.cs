using BasicCommerce.Application.Features.RewardPoints.Commands;
using BasicCommerce.Application.Features.RewardPoints.Queries;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.RewardPoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicCommerce.Backoffice.Api.Controllers;

[ApiController]
[Route("api/reward-points")]
[Authorize]
public class RewardPointsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RewardPointsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("settings")]
    public async Task<ActionResult<ApiResponse<RewardPointsSettingsResponse>>> GetSettings(
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRewardPointsSettingsQuery(), ct);
        return Ok(ApiResponse<RewardPointsSettingsResponse>.Ok(result));
    }

    [HttpPut("settings")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<RewardPointsSettingsResponse>>> UpdateSettings(
        [FromBody] UpdateRewardPointsSettingsRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateRewardPointsSettingsCommand(
            request.IsEnabled,
            request.ExchangeRate,
            request.MinimumPointsToUse,
            request.MaximumPointsPerOrder,
            request.MaximumRedeemedRate,
            request.PurchaseSpendPerPoint,
            request.PointsEarnedPerSpend,
            request.PurchasePointsValidityDays,
            request.MinimumOrderTotalForPoints,
            request.PointsForRegistration,
            request.RegistrationPointsValidityDays,
            request.ActivatePointsImmediately,
            request.DisplayHowMuchWillBeEarned,
            request.PointsAccumulatedForAllStores), ct);
        return Ok(ApiResponse<RewardPointsSettingsResponse>.Ok(result));
    }

    [HttpGet("customers/{customerId:guid}")]
    public async Task<ActionResult<ApiResponse<RewardPointsAccountResponse>>> GetCustomerPoints(
        Guid customerId, [FromQuery] Guid? storeId = null, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetCustomerRewardPointsQuery(customerId, storeId), ct);
        return Ok(ApiResponse<RewardPointsAccountResponse>.Ok(result));
    }

    /// <summary>
    /// Points snapshot for checkout: balance, value, the most usable on this order, and
    /// what the order will earn.
    /// </summary>
    [HttpGet("customers/{customerId:guid}/checkout")]
    public async Task<ActionResult<ApiResponse<CheckoutRewardPointsResponse>>> GetCheckoutPoints(
        Guid customerId, [FromQuery] Guid storeId, [FromQuery] decimal orderTotal,
        [FromQuery] decimal pointsAmount = 0, CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetCheckoutRewardPointsQuery(customerId, storeId, orderTotal, pointsAmount), ct);
        return Ok(ApiResponse<CheckoutRewardPointsResponse>.Ok(result));
    }

    [HttpPost("adjust")]
    [Authorize(Policy = "StoreManagerAndAbove")]
    public async Task<ActionResult<ApiResponse<RewardPointsAccountResponse>>> ManualAdjust(
        [FromBody] ManualAdjustRewardPointsRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ManualAdjustRewardPointsCommand(
            request.CustomerId, request.StoreId, request.Points, request.Notes), ct);
        return Ok(ApiResponse<RewardPointsAccountResponse>.Ok(result));
    }
}
