using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Entities;

public class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public EntityStatus Status { get; set; } = EntityStatus.Active;

    public void SetUpdated(Guid updatedBy)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void SoftDelete(Guid deletedBy)
    {
        Status = EntityStatus.Deleted;
        SetUpdated(deletedBy);
    }
}

public class TenantEntity : BaseEntity
{
    public Guid TenantId { get; set; }
}
