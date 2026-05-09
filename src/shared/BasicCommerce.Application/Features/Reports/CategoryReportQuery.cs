using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Products;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Reports;

public record CategoryReportQuery(
    bool IncludeInactive = false,
    string? Search = null) : IRequest<byte[]>;

public class CategoryReportQueryHandler : IRequestHandler<CategoryReportQuery, byte[]>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICategoryReportService _reportService;

    public CategoryReportQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ICategoryReportService reportService)
    {
        _uow = uow;
        _currentUser = currentUser;
        _reportService = reportService;
    }

    public async Task<byte[]> Handle(CategoryReportQuery request, CancellationToken ct)
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

        var list = filtered.ToList();

        var parentNames = all
            .Where(c => c.ParentCategoryId.HasValue)
            .Select(c => c.ParentCategoryId!.Value)
            .Distinct()
            .ToDictionary(id => id, id => all.FirstOrDefault(c => c.Id == id)?.Name);

        var childCounts = all
            .Where(c => c.ParentCategoryId.HasValue)
            .GroupBy(c => c.ParentCategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var rows = list.Select(c => new CategoryResponse(
            c.Id,
            c.Name,
            c.NameBn,
            c.Description,
            c.ParentCategoryId,
            c.ParentCategoryId.HasValue ? parentNames.GetValueOrDefault(c.ParentCategoryId.Value) : null,
            c.SortOrder,
            c.Status.ToString(),
            childCounts.GetValueOrDefault(c.Id)));

        return _reportService.Generate(rows, request.Search, request.IncludeInactive);
    }
}
