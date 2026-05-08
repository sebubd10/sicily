using BasicCommerce.Domain.Exceptions;

namespace BasicCommerce.Domain.Entities;

public class UserType : TenantEntity
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; }
    public int SortOrder { get; private set; }
    public string? Color { get; private set; }

    private readonly List<AppSubMenu> _menuAccess = [];
    public IReadOnlyCollection<AppSubMenu> MenuAccess => _menuAccess.AsReadOnly();

    private readonly List<ApiPermission> _permissions = [];
    public IReadOnlyCollection<ApiPermission> Permissions => _permissions.AsReadOnly();

    private UserType() { }

    public static UserType Create(Guid tenantId, string name, string? description = null,
        bool isSystem = false, int sortOrder = 0, string? color = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new UserType
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Description = description,
            IsSystem = isSystem,
            SortOrder = sortOrder,
            Color = color
        };
    }

    public void Update(string name, string? description, int sortOrder, string? color)
    {
        if (IsSystem)
            throw new DomainException("System user types cannot be renamed.");
        Name = name.Trim();
        Description = description;
        SortOrder = sortOrder;
        Color = color;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetMenuAccess(IEnumerable<AppSubMenu> subMenus)
    {
        _menuAccess.Clear();
        _menuAccess.AddRange(subMenus);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPermissions(IEnumerable<ApiPermission> permissions)
    {
        _permissions.Clear();
        _permissions.AddRange(permissions);
        UpdatedAt = DateTime.UtcNow;
    }
}
