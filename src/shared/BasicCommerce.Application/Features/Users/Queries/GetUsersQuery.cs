using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Users;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Users.Queries;

public record GetUsersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    bool IncludeInactive = false) : IRequest<PaginatedResponse<UserListResponse>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedResponse<UserListResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetUsersQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResponse<UserListResponse>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var users = (await _uow.Users.GetAllForTenantAsync(tenantId, ct)).ToList();

        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);

        IEnumerable<Domain.Entities.User> filtered = users.OrderBy(u => u.FullName);

        if (!request.IncludeInactive)
            filtered = filtered.Where(u => u.Status == EntityStatus.Active);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var q = request.Search.Trim().ToLowerInvariant();
            filtered = filtered.Where(u =>
                u.FullName.ToLowerInvariant().Contains(q) ||
                u.Email.ToLowerInvariant().Contains(q) ||
                u.EmployeeCode.ToLowerInvariant().Contains(q));
        }

        var totalCount = filtered.Count();
        var paged = filtered.Skip((page - 1) * size).Take(size).ToList();

        // Build lookup caches to avoid N+1 queries
        var storeCache = new Dictionary<Guid, string>();
        var userTypeCache = (await _uow.UserTypes.GetAllForTenantAsync(tenantId, ct))
            .ToDictionary(ut => ut.Id, ut => ut.Name);

        var result = new List<UserListResponse>();

        foreach (var user in paged)
        {
            string? storeName = null;
            if (user.StoreId.HasValue)
            {
                if (!storeCache.TryGetValue(user.StoreId.Value, out storeName))
                {
                    var store = await _uow.Stores.GetByIdAsync(user.StoreId.Value, ct);
                    storeName = store?.Name;
                    if (storeName is not null)
                        storeCache[user.StoreId.Value] = storeName;
                }
            }

            string? userTypeName = user.UserTypeId.HasValue
                ? userTypeCache.GetValueOrDefault(user.UserTypeId.Value)
                : null;

            result.Add(new UserListResponse(
                user.Id,
                user.EmployeeCode,
                user.FullName,
                user.Email,
                user.Role.ToString(),
                storeName,
                user.Status.ToString(),
                user.LastLoginAt,
                user.UserTypeId,
                userTypeName,
                user.IsLocked));
        }

        return new PaginatedResponse<UserListResponse>(result, totalCount, page, size);
    }
}
