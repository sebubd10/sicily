using BasicCommerce.Contracts.Stores;
using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Application.Features.Terminals;

internal static class TerminalMapper
{
    internal static TerminalResponse ToResponse(Terminal t, string storeName) =>
        new(
            t.Id,
            t.StoreId,
            storeName,
            t.Name,
            t.Code,
            t.Type.ToString(),
            t.Status.ToString(),
            t.TerminalStatus.ToString(),
            null);
}
