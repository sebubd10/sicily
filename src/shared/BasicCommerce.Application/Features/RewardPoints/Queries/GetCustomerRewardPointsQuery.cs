using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.RewardPoints;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.RewardPoints.Queries;

public record GetCustomerRewardPointsQuery(Guid CustomerId, Guid? StoreId = null)
    : IRequest<RewardPointsAccountResponse>;

public class GetCustomerRewardPointsQueryHandler
    : IRequestHandler<GetCustomerRewardPointsQuery, RewardPointsAccountResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetCustomerRewardPointsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<RewardPointsAccountResponse> Handle(
        GetCustomerRewardPointsQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var settings = await _uow.RewardPointsSettings.GetByTenantAsync(tenantId, ct);
        var storeId = settings?.PointsAccumulatedForAllStores == false ? request.StoreId : null;

        var account = await _uow.RewardPointsAccounts.GetWithEntriesAsync(
            tenantId, request.CustomerId, storeId, ct)
            ?? throw new NotFoundException("RewardPointsAccount", request.CustomerId);

        return MapToResponse(account);
    }

    internal static RewardPointsAccountResponse MapToResponse(
        Domain.Entities.RewardPointsAccount a) => new(
        a.Id,
        a.CustomerId,
        a.StoreId,
        a.TotalEarnedPoints,
        a.UsedPoints,
        a.ExpiredPoints,
        a.PendingPoints,
        a.AvailablePoints,
        a.Entries.Select(e => new RewardPointsEntryResponse(
            e.Id, e.Points, e.EntryType.ToString(), e.IsActivated,
            e.ExpiresAt, e.TransactionId, e.Notes, e.CreatedAt)).ToList());
}
