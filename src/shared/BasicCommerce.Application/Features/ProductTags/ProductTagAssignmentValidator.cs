using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;

namespace BasicCommerce.Application.Features.ProductTags;

public static class ProductTagAssignmentValidator
{
    public static void EnsureNoNewInactiveTags(
        IEnumerable<ProductTag> requestedTags, IEnumerable<Guid> existingTagIds)
    {
        var existing = existingTagIds.ToHashSet();
        var blocked = requestedTags
            .Where(t => t.Status != EntityStatus.Active && !existing.Contains(t.Id))
            .ToList();

        if (blocked.Count > 0)
            throw new DomainException(
                $"Cannot assign inactive tag{(blocked.Count == 1 ? "" : "s")}: {string.Join(", ", blocked.Select(t => t.Name))}.");
    }
}
