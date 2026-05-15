using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Manufacturers;
using BasicCommerce.Domain.Interfaces;
using MediatR;

namespace BasicCommerce.Application.Features.Reports;

public record ManufacturerReportQuery(
    string? Search = null,
    bool IncludeInactive = false) : IRequest<byte[]>;

public class ManufacturerReportQueryHandler : IRequestHandler<ManufacturerReportQuery, byte[]>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IManufacturerReportService _reportService;

    public ManufacturerReportQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IManufacturerReportService reportService)
    {
        _uow = uow;
        _currentUser = currentUser;
        _reportService = reportService;
    }

    public async Task<byte[]> Handle(ManufacturerReportQuery request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var all = (await _uow.Manufacturers.GetAllForTenantAsync(tenantId, ct))
            .OrderBy(m => m.Name)
            .AsEnumerable();

        if (!request.IncludeInactive)
            all = all.Where(m => m.Status == Domain.Enums.EntityStatus.Active);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var q = request.Search.Trim().ToLowerInvariant();
            all = all.Where(m =>
                m.Name.ToLowerInvariant().Contains(q) ||
                (m.Code ?? string.Empty).ToLowerInvariant().Contains(q) ||
                (m.Country ?? string.Empty).ToLowerInvariant().Contains(q) ||
                (m.ContactEmail ?? string.Empty).ToLowerInvariant().Contains(q));
        }

        var rows = all.Select(m => new ManufacturerReportRow(
            m.Name, m.Code, m.Country, m.Website, m.ContactEmail, m.Notes, m.Status.ToString()
        )).ToList();

        return _reportService.Generate(rows, request.Search, request.IncludeInactive);
    }
}
