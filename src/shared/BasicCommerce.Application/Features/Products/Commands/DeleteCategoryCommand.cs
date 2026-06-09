using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Commands;

public record DeleteCategoryCommand(Guid CategoryId) : IRequest;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteCategoryCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var category = await _uow.Categories.GetByIdAsync(request.CategoryId, ct)
            ?? throw new NotFoundException("Category", request.CategoryId);

        if (category.TenantId != tenantId)
            throw new NotFoundException("Category", request.CategoryId);

        var children = await _uow.Categories.GetChildrenAsync(tenantId, category.Id, ct);
        if (children.Any())
            throw new DomainException(
                $"Cannot delete: this category has {children.Count()} sub-categor{(children.Count() == 1 ? "y" : "ies")}. Delete or move them first.");

        var productCount = await _uow.Products.CountByCategoryAsync(tenantId, category.Id, ct);
        if (productCount > 0)
            throw new DomainException(
                $"Cannot delete: {productCount} product{(productCount == 1 ? " is" : "s are")} assigned to this category. Reassign or deactivate them first.");

        _uow.Categories.Remove(category);
        await _uow.SaveChangesAsync(ct);
    }
}
