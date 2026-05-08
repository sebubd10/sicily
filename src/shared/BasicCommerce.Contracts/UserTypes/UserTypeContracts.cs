namespace BasicCommerce.Contracts.UserTypes;

public record UserTypeResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystem,
    int SortOrder,
    string? Color,
    IReadOnlyList<Guid>? AllowedSubMenuIds = null,
    IReadOnlyList<string>? PermissionCodes = null);

public record CreateUserTypeRequest(
    string Name,
    string? Description = null,
    int SortOrder = 0,
    string? Color = null);

public record UpdateUserTypeRequest(
    string Name,
    string? Description = null,
    int SortOrder = 0,
    string? Color = null);

public record SetUserTypeMenusRequest(IReadOnlyList<Guid> SubMenuIds);

public record SetUserTypePermissionsRequest(IReadOnlyList<string> PermissionCodes);

public record AssignUserTypeRequest(Guid? UserTypeId);

public record MenuResponse(
    Guid Id,
    string Name,
    string? Icon,
    int SortOrder,
    IReadOnlyList<SubMenuResponse> Items);

public record SubMenuResponse(
    Guid Id,
    string Name,
    string? Icon,
    string Route,
    string? PermissionCode,
    int SortOrder);

public record ApiPermissionResponse(
    Guid Id,
    string Code,
    string Name,
    string Group,
    string? Description);
