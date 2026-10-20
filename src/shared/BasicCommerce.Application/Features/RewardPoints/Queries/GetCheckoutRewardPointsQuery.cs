using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.RewardPoints;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.RewardPoints.Queries;

/// <param name="OrderTotal">Total of the cart being checked out.</param>
/// <param name="PointsAmount">Part of the total the customer is paying with points.</param>
public record GetCheckoutRewardPointsQuery(
    Guid CustomerId,
    Guid StoreId,
    decimal OrderTotal,
    decimal PointsAmount = 0) : IRequest<CheckoutRewardPointsResponse>;

public class GetCheckoutRewardPointsQueryHandler
    : IRequestHandler<GetCheckoutRewardPointsQuery, CheckoutRewardPointsResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetCheckoutRewardPointsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<CheckoutRewardPointsResponse> Handle(
        GetCheckoutRewardPointsQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var settings = await _uow.RewardPointsSettings.GetByTenantAsync(tenantId, ct);

        if (settings is null || settings.Status != EntityStatus.Active)
            return new CheckoutRewardPointsResponse(
                false, 0, 0, 0, settings?.ExchangeRate ?? 1m, 0, 0, 0, 0, true);

        var account = await RewardPointsLedger.GetAccountAsync(
            _uow, settings, tenantId, request.CustomerId, request.StoreId, ct);

        var available = 0;
        var pending = 0;
        if (account is not null)
        {
            // Read-only: expiry is applied to the tracked entity so the numbers are right,
            // but nothing is saved here. It's persisted on the next redeem/earn.
            await RewardPointsLedger.ApplyExpiryAsync(_uow, account, ct);
            available = Math.Max(0, account.AvailablePoints);
            pending = account.PendingPoints;
        }

        var orderTotal = Math.Max(0, request.OrderTotal);
        var maxPoints = orderTotal > 0
            ? settings.CalculateMaxRedeemablePoints(available, orderTotal)
            : 0;
        var maxAmount = Math.Min(orderTotal,
            Math.Round(maxPoints * settings.ExchangeRate, 2, MidpointRounding.ToZero));

        var pointsToEarn = settings.CalculatePurchasePoints(
            Math.Max(0, orderTotal - Math.Max(0, request.PointsAmount)));

        return new CheckoutRewardPointsResponse(
            ProgramEnabled: true,
            AvailablePoints: available,
            AvailableValue: Math.Round(available * settings.ExchangeRate, 2),
            PendingPoints: pending,
            ExchangeRate: settings.ExchangeRate,
            MinimumPointsToUse: settings.MinimumPointsToUse,
            MaxRedeemablePoints: maxPoints,
            MaxRedeemableAmount: maxAmount,
            PointsToEarn: pointsToEarn,
            PointsActivateImmediately: settings.ActivatePointsImmediately);
    }
}
