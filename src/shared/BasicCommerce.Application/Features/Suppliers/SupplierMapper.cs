using BasicCommerce.Contracts.Suppliers;
using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Application.Features.Suppliers;

internal static class SupplierMapper
{
    internal static SupplierResponse ToResponse(Supplier s) =>
        new(s.Id, s.Name, s.Code, s.ContactName, s.Email, s.Phone,
            s.Address?.Line1, s.Address?.City, s.LeadTimeDays, s.Notes,
            s.ManufacturerId, s.Manufacturer?.Name, s.Status.ToString());
}
