using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.RewardPoints;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.RewardPoints.Commands;

public record ManualAdjustRewardPointsCommand(
    Guid CustomerId,
    Guid? StoreId,
    int Points,
    string Notes) : IRequest<RewardPointsAccountResponse>;

public class ManualAdjustRewardPointsCommandValidator
    : AbstractValidator<ManualAdjustRewardPointsCommand>
{
    public ManualAdjustRewardPointsCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Points).NotEqual(0).WithMessage("Points adjustment cannot be zero.");
        RuleFor(x => x.Notes).NotEmpty().MaximumLength(500);
    }
}

public class ManualAdjustRewardPointsCommandHandler
    : IRequestHandler<ManualAdjustRewardPointsCommand, RewardPointsAccountResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public ManualAdjustRewardPointsCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<RewardPointsAccountResponse> Handle(
        ManualAdjustRewardPointsCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var settings = await _uow.RewardPointsSettings.GetByTenantAsync(tenantId, ct);
        var storeId = settings?.PointsAccumulatedForAllStores == false ? request.StoreId : null;

        var account = await _uow.RewardPointsAccounts.GetWithEntriesAsync(
            tenantId, request.CustomerId, storeId, ct);

        if (account is null)
        {
            account = RewardPointsAccount.Create(tenantId, request.CustomerId, storeId);
            await _uow.RewardPointsAccounts.AddAsync(account, ct);
        }

        account.ManualAdjustment(request.Points, request.Notes);

        var customer = await _uow.Customers.GetByIdAsync(request.CustomerId, ct);
        if (customer is not null)
        {
            if (request.Points > 0) customer.AddLoyaltyPoints(request.Points);
            else customer.RedeemLoyaltyPoints(Math.Abs(request.Points));
            _uow.Customers.Update(customer);
        }

        _uow.RewardPointsAccounts.Update(account);
        await _uow.SaveChangesAsync(ct);

        return GetCustomerRewardPointsQueryHandler.MapToResponse(account);
    }
}
