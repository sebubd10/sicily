using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Users;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Users.Queries;

public record GetUsersQuery(int Page = 1, int PageSize = 20) : IRequest<IEnumerable<UserListResponse>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IEnumerable<UserListResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetUsersQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<UserListResponse>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var users = await _uow.Users.GetAllForTenantAsync(tenantId, ct);

        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);

        var paged = users
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * size)
            .Take(size);

        var storeCache = new Dictionary<Guid, string>();
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

            result.Add(new UserListResponse(
                user.Id,
                user.EmployeeCode,
                user.FullName,
                user.Email,
                user.Role.ToString(),
                storeName,
                user.IsActive,
                user.LastLoginAt));
        }

        return result;
    }
}
