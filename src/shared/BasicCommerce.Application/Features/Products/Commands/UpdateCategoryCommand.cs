using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Commands;

public record UpdateCategoryCommand(
    Guid CategoryId,
    string Name,
    string NameBn,
    string? Description,
    int SortOrder) : IRequest<CategoryResponse>;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NameBn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateCategoryCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<CategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var category = await _uow.Categories.GetByIdAsync(request.CategoryId, ct)
            ?? throw new NotFoundException("Category", request.CategoryId);

        if (category.TenantId != tenantId)
            throw new NotFoundException("Category", request.CategoryId);

        if (await _uow.Categories.NameExistsAsync(tenantId, request.Name, request.CategoryId, ct))
            throw new DomainException($"Category name '{request.Name}' is already in use.");

        category.Update(request.Name, request.NameBn, request.Description, request.SortOrder);
        await _uow.SaveChangesAsync(ct);

        var children = await _uow.Categories.GetChildrenAsync(tenantId, category.Id, ct);
        string? parentName = null;
        if (category.ParentCategoryId.HasValue)
        {
            var parent = await _uow.Categories.GetByIdAsync(category.ParentCategoryId.Value, ct);
            parentName = parent?.Name;
        }

        return new CategoryResponse(
            category.Id,
            category.Name,
            category.NameBn,
            category.Description,
            category.ParentCategoryId,
            parentName,
            category.SortOrder,
            category.Status.ToString(),
            children.Count());
    }
}
