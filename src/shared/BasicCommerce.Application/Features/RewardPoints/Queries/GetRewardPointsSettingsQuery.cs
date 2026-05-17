using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.RewardPoints;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.RewardPoints.Queries;

public record GetRewardPointsSettingsQuery : IRequest<RewardPointsSettingsResponse>;

public class GetRewardPointsSettingsQueryHandler
    : IRequestHandler<GetRewardPointsSettingsQuery, RewardPointsSettingsResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetRewardPointsSettingsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<RewardPointsSettingsResponse> Handle(
        GetRewardPointsSettingsQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var settings = await _uow.RewardPointsSettings.GetByTenantAsync(tenantId, ct);

        if (settings is null)
        {
            settings = RewardPointsSettings.CreateDefault(tenantId);
            await _uow.RewardPointsSettings.AddAsync(settings, ct);
            await _uow.SaveChangesAsync(ct);
        }

        return MapToResponse(settings);
    }

    internal static RewardPointsSettingsResponse MapToResponse(RewardPointsSettings s) => new(
        s.Id,
        s.Status == EntityStatus.Active,
        s.ExchangeRate,
        s.MinimumPointsToUse,
        s.MaximumPointsPerOrder,
        s.MaximumRedeemedRate,
        s.PurchaseSpendPerPoint,
        s.PointsEarnedPerSpend,
        s.PurchasePointsValidityDays,
        s.MinimumOrderTotalForPoints,
        s.PointsForRegistration,
        s.RegistrationPointsValidityDays,
        s.ActivatePointsImmediately,
        s.DisplayHowMuchWillBeEarned,
        s.PointsAccumulatedForAllStores);
}
