using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Users;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Reports;

public record UserReportQuery(string? Search = null) : IRequest<byte[]>;

public class UserReportQueryHandler : IRequestHandler<UserReportQuery, byte[]>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IUserReportService _reportService;

    public UserReportQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IUserReportService reportService)
    {
        _uow = uow;
        _currentUser = currentUser;
        _reportService = reportService;
    }

    public async Task<byte[]> Handle(UserReportQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var users = (await _uow.Users.GetAllForTenantAsync(tenantId, ct)).ToList();

        IEnumerable<Domain.Entities.User> filtered = users.OrderBy(u => u.FullName);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var q = request.Search.Trim().ToLowerInvariant();
            filtered = filtered.Where(u =>
                u.FullName.ToLowerInvariant().Contains(q) ||
                u.Email.ToLowerInvariant().Contains(q) ||
                u.EmployeeCode.ToLowerInvariant().Contains(q));
        }

        var userTypeCache = (await _uow.UserTypes.GetAllForTenantAsync(tenantId, ct))
            .ToDictionary(ut => ut.Id, ut => ut.Name);

        var storeCache = new Dictionary<Guid, string>();

        var rows = new List<UserReportRow>();
        foreach (var u in filtered)
        {
            string? storeName = null;
            if (u.StoreId.HasValue)
            {
                if (!storeCache.TryGetValue(u.StoreId.Value, out storeName))
                {
                    var store = await _uow.Stores.GetByIdAsync(u.StoreId.Value, ct);
                    storeName = store?.Name;
                    if (storeName is not null)
                        storeCache[u.StoreId.Value] = storeName;
                }
            }

            string? userTypeName = u.UserTypeId.HasValue
                ? userTypeCache.GetValueOrDefault(u.UserTypeId.Value)
                : null;

            rows.Add(new UserReportRow(
                u.EmployeeCode,
                u.FullName,
                u.Email,
                u.Role.ToString(),
                userTypeName,
                storeName,
                u.Status.ToString(),
                u.IsLocked,
                u.LastLoginAt));
        }

        return _reportService.Generate(rows, request.Search);
    }
}
