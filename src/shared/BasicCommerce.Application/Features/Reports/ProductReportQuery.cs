using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Reports;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Reports;

public record ProductReportQuery(
    string? Search,
    bool IncludeInactive,
    Guid? CategoryId = null) : IRequest<byte[]>;

public class ProductReportQueryHandler : IRequestHandler<ProductReportQuery, byte[]>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IProductReportService _reportService;

    public ProductReportQueryHandler(
        IUnitOfWork uow, ICurrentUserService currentUser, IProductReportService reportService)
    {
        _uow = uow;
        _currentUser = currentUser;
        _reportService = reportService;
    }

    public async Task<byte[]> Handle(ProductReportQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var products = await _uow.Products.GetAllForTenantAsync(
            tenantId, request.CategoryId, request.IncludeInactive, request.Search, ct);

        var vatRates = new Dictionary<Guid, decimal>();
        foreach (var p in products)
        {
            if (!vatRates.ContainsKey(p.VatRateId))
            {
                var vr = await _uow.VatRates.GetByIdAsync(p.VatRateId, ct);
                if (vr is not null) vatRates[p.VatRateId] = vr.Rate;
            }
        }

        string? categoryName = null;
        if (request.CategoryId.HasValue)
        {
            var cat = await _uow.Categories.GetByIdAsync(request.CategoryId.Value, ct);
            categoryName = cat?.Name;
        }

        var rows = products.Select(p => new ProductReportRow(
            p.Sku, p.Barcode, p.Name,
            p.Category?.Name ?? "—",
            p.Price.Amount,
            p.CostPrice?.Amount,
            p.Price.Currency,
            vatRates.GetValueOrDefault(p.VatRateId),
            p.UnitType.ToString(),
            p.Manufacturer?.Name,
            p.Status.ToString()));

        return _reportService.Generate(rows, request.Search, request.IncludeInactive, categoryName);
    }
}
