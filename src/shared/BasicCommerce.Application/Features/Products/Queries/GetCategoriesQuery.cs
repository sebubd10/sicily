using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Queries;

public record GetCategoriesQuery(bool IncludeInactive = false) : IRequest<IEnumerable<CategoryResponse>>;

public record GetCategoryQuery(Guid CategoryId) : IRequest<CategoryResponse>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IEnumerable<CategoryResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetCategoriesQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<CategoryResponse>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var all = await _uow.Categories.GetAllForTenantAsync(tenantId, ct);

        if (!request.IncludeInactive)
            all = all.Where(c => c.IsActive);

        var list = all.ToList();
        var parentNames = list
            .Where(c => c.ParentCategoryId.HasValue)
            .Select(c => c.ParentCategoryId!.Value)
            .Distinct()
            .ToDictionary(id => id, id => list.FirstOrDefault(c => c.Id == id)?.Name);

        var childCounts = list
            .Where(c => c.ParentCategoryId.HasValue)
            .GroupBy(c => c.ParentCategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        return list.Select(c => new CategoryResponse(
            c.Id,
            c.Name,
            c.NameBn,
            c.Description,
            c.ParentCategoryId,
            c.ParentCategoryId.HasValue
                ? parentNames.GetValueOrDefault(c.ParentCategoryId.Value)
                : null,
            c.SortOrder,
            c.IsActive,
            childCounts.GetValueOrDefault(c.Id)));
    }
}

public class GetCategoryQueryHandler : IRequestHandler<GetCategoryQuery, CategoryResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetCategoryQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<CategoryResponse> Handle(GetCategoryQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var category = await _uow.Categories.GetByIdAsync(request.CategoryId, ct)
            ?? throw new BasicCommerce.Domain.Exceptions.NotFoundException("Category", request.CategoryId);

        if (category.TenantId != tenantId)
            throw new BasicCommerce.Domain.Exceptions.NotFoundException("Category", request.CategoryId);

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
            category.IsActive,
            children.Count());
    }
}
