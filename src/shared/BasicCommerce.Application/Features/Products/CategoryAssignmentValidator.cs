using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;

namespace BasicCommerce.Application.Features.Products;

public static class CategoryAssignmentValidator
{
    public static void EnsureActiveOrUnchanged(Category category, Guid? previousCategoryId)
    {
        if (category.Status == EntityStatus.Active || category.Id == previousCategoryId)
            return;

        throw new DomainException(
            $"Cannot assign {category.Status.ToString().ToLowerInvariant()} category: {category.Name}.");
    }
}
