using BasicCommerce.Contracts.Stores;
using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Application.Features.Stores;

internal static class StoreMapper
{
    internal static StoreResponse ToResponse(Store store, int terminalCount) =>
        new(
            store.Id,
            store.Name,
            store.Code,
            $"{store.Address.Line1}, {store.Address.City}",
            store.Phone,
            store.Email,
            store.Status.ToString(),
            terminalCount,
            store.Address.Line1,
            store.Address.Line2,
            store.Address.City,
            store.Address.District,
            store.Address.PostalCode,
            store.Address.Country,
            store.OpeningTime.ToString("HH:mm"),
            store.ClosingTime.ToString("HH:mm"));
}
