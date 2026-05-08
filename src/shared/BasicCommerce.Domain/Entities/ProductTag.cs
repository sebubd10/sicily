namespace BasicCommerce.Domain.Entities;

public class ProductTag : TenantEntity
{
    public string Name { get; private set; } = default!;

    private readonly List<Product> _products = [];
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private ProductTag() { }

    public static ProductTag Create(Guid tenantId, string name) =>
        new() { TenantId = tenantId, Name = name.Trim() };

    public void Rename(string name)
    {
        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
