namespace BasicCommerce.Domain.Entities;

public class Category : TenantEntity
{
    public string Name { get; private set; } = default!;
    public string NameBn { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? ImageUrl { get; private set; }

    private readonly List<Product> _products = [];
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private Category() { }

    public static Category Create(Guid tenantId, string name, string nameBn,
        Guid? parentCategoryId = null, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(nameBn);

        return new Category
        {
            TenantId = tenantId,
            Name = name,
            NameBn = nameBn,
            ParentCategoryId = parentCategoryId,
            Description = description
        };
    }

    public void Update(string name, string nameBn, string? description, int sortOrder)
    {
        Name = name;
        NameBn = nameBn;
        Description = description;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;
}
