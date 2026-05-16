using BasicCommerce.Contracts.Reports;

namespace BasicCommerce.Application.Interfaces;

public interface IProductReportService
{
    byte[] Generate(IEnumerable<ProductReportRow> products, string? search = null,
        bool includeInactive = false, string? categoryFilter = null);
}
