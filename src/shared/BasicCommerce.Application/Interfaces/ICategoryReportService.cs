using BasicCommerce.Contracts.Products;

namespace BasicCommerce.Application.Interfaces;

public interface ICategoryReportService
{
    byte[] Generate(
        IEnumerable<CategoryResponse> categories,
        string? search = null,
        bool includeInactive = false);
}
