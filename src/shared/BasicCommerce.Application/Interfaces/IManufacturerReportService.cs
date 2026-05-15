using BasicCommerce.Contracts.Manufacturers;

namespace BasicCommerce.Application.Interfaces;

public interface IManufacturerReportService
{
    byte[] Generate(
        IEnumerable<ManufacturerReportRow> manufacturers,
        string? search = null,
        bool includeInactive = false);
}
