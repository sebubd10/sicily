using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public Guid? CreatedBy { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }
    public EntityStatus Status { get; protected set; } = EntityStatus.Active;

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

public abstract class TenantEntity : BaseEntity
{
    public Guid TenantId { get; protected set; }
}
