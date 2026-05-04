using BasicCommerce.Contracts.Manufacturers;
using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Application.Features.Manufacturers;

internal static class ManufacturerMapper
{
    internal static ManufacturerResponse ToResponse(Manufacturer m) =>
        new(m.Id, m.Name, m.Code, m.Country, m.Website, m.ContactEmail, m.Notes, m.Status.ToString());
}
