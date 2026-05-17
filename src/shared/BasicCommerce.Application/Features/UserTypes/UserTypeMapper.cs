using BasicCommerce.Contracts.UserTypes;
using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Application.Features.UserTypes;

public static class UserTypeMapper
{
    public static UserTypeResponse ToResponse(UserType ut)
        => new(ut.Id, ut.Name, ut.Description, ut.IsSystem, ut.SortOrder, ut.Color);

    public static UserTypeResponse ToResponseWithAccess(UserType ut)
        => new(ut.Id, ut.Name, ut.Description, ut.IsSystem, ut.SortOrder, ut.Color,
            ut.MenuAccess.Select(sm => sm.Id).ToList().AsReadOnly(),
            ut.Permissions.Select(p => p.Code).ToList().AsReadOnly());

    public static MenuResponse ToMenuResponse(AppMenu menu)
        => new(menu.Id, menu.Name, menu.Icon, menu.SortOrder,
            menu.SubMenus.OrderBy(sm => sm.SortOrder)
                .Select(sm => new SubMenuResponse(
                    sm.Id, sm.Name, sm.Icon, sm.Route, sm.PermissionCode, sm.SortOrder))
                .ToList().AsReadOnly());
}
