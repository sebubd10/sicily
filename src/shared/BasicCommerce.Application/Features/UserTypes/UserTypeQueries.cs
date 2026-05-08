using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.UserTypes;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.UserTypes;

public record GetUserTypesQuery : IRequest<IReadOnlyList<UserTypeResponse>>;

public class GetUserTypesQueryHandler
    : IRequestHandler<GetUserTypesQuery, IReadOnlyList<UserTypeResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetUserTypesQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<IReadOnlyList<UserTypeResponse>> Handle(
        GetUserTypesQuery request, CancellationToken ct)
    {
        var userTypes = await _uow.UserTypes.GetAllWithAccessAsync(_currentUser.TenantId, ct);
        return userTypes.OrderBy(t => t.SortOrder)
            .Select(UserTypeMapper.ToResponseWithAccess).ToList().AsReadOnly();
    }
}

public record GetUserTypeQuery(Guid Id) : IRequest<UserTypeResponse>;

public class GetUserTypeQueryHandler : IRequestHandler<GetUserTypeQuery, UserTypeResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetUserTypeQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<UserTypeResponse> Handle(
        GetUserTypeQuery request, CancellationToken ct)
    {
        var userType = await _uow.UserTypes.GetWithAccessAsync(
            _currentUser.TenantId, request.Id, ct)
            ?? throw new NotFoundException("UserType", request.Id);
        return UserTypeMapper.ToResponseWithAccess(userType);
    }
}

// ── Menus query (returns menus + submenus accessible to current user) ─
public record GetMenusQuery : IRequest<IReadOnlyList<MenuResponse>>;

public class GetMenusQueryHandler : IRequestHandler<GetMenusQuery, IReadOnlyList<MenuResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetMenusQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<IReadOnlyList<MenuResponse>> Handle(
        GetMenusQuery request, CancellationToken ct)
    {
        var allMenus = await _uow.AppMenus.GetAllWithSubMenusAsync(ct);

        if (!_currentUser.UserTypeId.HasValue)
            return allMenus.OrderBy(m => m.SortOrder)
                .Select(UserTypeMapper.ToMenuResponse).ToList().AsReadOnly();

        var userType = await _uow.UserTypes.GetWithAccessAsync(
            _currentUser.TenantId, _currentUser.UserTypeId.Value, ct);

        var allowedSubMenuIds = userType?.MenuAccess.Select(sm => sm.Id).ToHashSet()
            ?? [];

        return allMenus.OrderBy(m => m.SortOrder)
            .Select(menu =>
            {
                var subMenus = menu.SubMenus
                    .Where(sm => allowedSubMenuIds.Count == 0 || allowedSubMenuIds.Contains(sm.Id))
                    .OrderBy(sm => sm.SortOrder)
                    .Select(sm => new SubMenuResponse(sm.Id, sm.Name, sm.Icon, sm.Route, sm.PermissionCode, sm.SortOrder))
                    .ToList();
                return new MenuResponse(menu.Id, menu.Name, menu.Icon, menu.SortOrder, subMenus);
            })
            .Where(m => m.Items.Count > 0)
            .ToList().AsReadOnly();
    }
}

// ── All permissions query ──────────────────────────────────────
public record GetApiPermissionsQuery(string? Group) : IRequest<IReadOnlyList<ApiPermissionResponse>>;

public class GetApiPermissionsQueryHandler
    : IRequestHandler<GetApiPermissionsQuery, IReadOnlyList<ApiPermissionResponse>>
{
    private readonly IUnitOfWork _uow;

    public GetApiPermissionsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<ApiPermissionResponse>> Handle(
        GetApiPermissionsQuery request, CancellationToken ct)
    {
        var perms = string.IsNullOrWhiteSpace(request.Group)
            ? await _uow.ApiPermissions.GetAllAsync(ct)
            : await _uow.ApiPermissions.GetByGroupAsync(request.Group, ct);

        return perms.OrderBy(p => p.Group).ThenBy(p => p.Name)
            .Select(p => new ApiPermissionResponse(p.Id, p.Code, p.Name, p.Group, p.Description))
            .ToList().AsReadOnly();
    }
}
