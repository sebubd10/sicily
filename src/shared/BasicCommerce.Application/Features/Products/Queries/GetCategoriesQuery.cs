using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Common;
using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Products.Queries;

public record GetCategoriesQuery(
    bool IncludeInactive = false,
    int PageNumber = 1,
    int PageSize = 20,
    string? Search = null) : IRequest<PaginatedResponse<CategoryResponse>>;

public record GetCategoryQuery(Guid CategoryId) : IRequest<CategoryResponse>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, PaginatedResponse<CategoryResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetCategoriesQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResponse<CategoryResponse>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var all = (await _uow.Categories.GetAllForTenantAsync(tenantId, ct)).ToList();

        IEnumerable<BasicCommerce.Domain.Entities.Category> filtered = all;

        if (!request.IncludeInactive)
            filtered = filtered.Where(c => c.Status == EntityStatus.Active);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var q = request.Search.Trim().ToLowerInvariant();
            filtered = filtered.Where(c =>
                c.Name.ToLowerInvariant().Contains(q) ||
                (c.NameBn != null && c.NameBn.Contains(request.Search.Trim())));
        }

        var list = filtered.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToList();
        var totalCount = list.Count;

        // Build lookup maps from the full set so parent names and child counts are correct
        var parentNames = all
            .Where(c => c.ParentCategoryId.HasValue)
            .Select(c => c.ParentCategoryId!.Value)
            .Distinct()
            .ToDictionary(id => id, id => all.FirstOrDefault(c => c.Id == id)?.Name);

        var childCounts = all
            .Where(c => c.ParentCategoryId.HasValue)
            .GroupBy(c => c.ParentCategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var pageSize   = Math.Max(1, Math.Min(request.PageSize, 200));
        var pageNumber = Math.Max(1, request.PageNumber);

        var items = list
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CategoryResponse(
                c.Id,
                c.Name,
                c.NameBn,
                c.Description,
                c.ParentCategoryId,
                c.ParentCategoryId.HasValue ? parentNames.GetValueOrDefault(c.ParentCategoryId.Value) : null,
                c.SortOrder,
                c.Status.ToString(),
                childCounts.GetValueOrDefault(c.Id)));

        return new PaginatedResponse<CategoryResponse>(items, totalCount, pageNumber, pageSize);
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
            category.Status.ToString(),
            children.Count());
    }
}
