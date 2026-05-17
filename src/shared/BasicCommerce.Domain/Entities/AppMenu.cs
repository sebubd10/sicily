namespace BasicCommerce.Domain.Entities;

public class AppMenu : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string? Icon { get; private set; }
    public int SortOrder { get; private set; }

    private readonly List<AppSubMenu> _subMenus = [];
    public IReadOnlyCollection<AppSubMenu> SubMenus => _subMenus.AsReadOnly();

    private AppMenu() { }

    public static AppMenu Create(string name, string? icon = null, int sortOrder = 0)
        => new() { Name = name, Icon = icon, SortOrder = sortOrder };

    public void Update(string name, string? icon, int sortOrder)
    {
        Name = name;
        Icon = icon;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class AppSubMenu : BaseEntity
{
    public Guid MenuId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Icon { get; private set; }
    public string Route { get; private set; } = default!;
    public string? PermissionCode { get; private set; }
    public int SortOrder { get; private set; }

    private AppSubMenu() { }

    public static AppSubMenu Create(Guid menuId, string name, string route,
        string? permissionCode = null, string? icon = null, int sortOrder = 0)
        => new()
        {
            MenuId = menuId,
            Name = name,
            Route = route,
            PermissionCode = permissionCode,
            Icon = icon,
            SortOrder = sortOrder
        };

    public void Update(string name, string route, string? permissionCode,
        string? icon, int sortOrder)
    {
        Name = name;
        Route = route;
        PermissionCode = permissionCode;
        Icon = icon;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }
}
