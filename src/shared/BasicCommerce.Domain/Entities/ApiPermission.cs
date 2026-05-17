namespace BasicCommerce.Domain.Entities;

public class ApiPermission : BaseEntity
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Group { get; private set; } = default!;
    public string? Description { get; private set; }

    private ApiPermission() { }

    public static ApiPermission Create(string code, string name, string group,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return new ApiPermission
        {
            Code = code.ToLowerInvariant(),
            Name = name,
            Group = group,
            Description = description
        };
    }

    public void Update(string name, string group, string? description)
    {
        Name = name;
        Group = group;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
}
