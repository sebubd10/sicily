using BasicCommerce.Application.Features.RewardPoints.Queries;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.RewardPoints;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.RewardPoints.Commands;

public record UpdateRewardPointsSettingsCommand(
    bool IsEnabled,
    decimal ExchangeRate,
    int MinimumPointsToUse,
    int MaximumPointsPerOrder,
    decimal MaximumRedeemedRate,
    decimal PurchaseSpendPerPoint,
    int PointsEarnedPerSpend,
    int PurchasePointsValidityDays,
    decimal MinimumOrderTotalForPoints,
    int PointsForRegistration,
    int RegistrationPointsValidityDays,
    bool ActivatePointsImmediately,
    bool DisplayHowMuchWillBeEarned,
    bool PointsAccumulatedForAllStores) : IRequest<RewardPointsSettingsResponse>;

public class UpdateRewardPointsSettingsCommandValidator
    : AbstractValidator<UpdateRewardPointsSettingsCommand>
{
    public UpdateRewardPointsSettingsCommandValidator()
    {
        RuleFor(x => x.ExchangeRate).GreaterThan(0);
        RuleFor(x => x.MinimumPointsToUse).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaximumPointsPerOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaximumRedeemedRate).InclusiveBetween(0m, 1m);
        RuleFor(x => x.PurchaseSpendPerPoint).GreaterThan(0);
        RuleFor(x => x.PointsEarnedPerSpend).GreaterThan(0);
        RuleFor(x => x.PurchasePointsValidityDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinimumOrderTotalForPoints).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PointsForRegistration).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RegistrationPointsValidityDays).GreaterThanOrEqualTo(0);
    }
}

public class UpdateRewardPointsSettingsCommandHandler
    : IRequestHandler<UpdateRewardPointsSettingsCommand, RewardPointsSettingsResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateRewardPointsSettingsCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<RewardPointsSettingsResponse> Handle(
        UpdateRewardPointsSettingsCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var settings = await _uow.RewardPointsSettings.GetByTenantAsync(tenantId, ct);

        if (settings is null)
        {
            settings = RewardPointsSettings.CreateDefault(tenantId);
            await _uow.RewardPointsSettings.AddAsync(settings, ct);
        }

        settings.Update(
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
            request.PointsAccumulatedForAllStores);

        if (request.IsEnabled) settings.Activate();
        else settings.Deactivate();

        _uow.RewardPointsSettings.Update(settings);
        await _uow.SaveChangesAsync(ct);

        return GetRewardPointsSettingsQueryHandler.MapToResponse(settings);
    }
}
