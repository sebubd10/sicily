using BasicCommerce.Contracts.GiftCards;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Interfaces;

namespace BasicCommerce.Application.Features.GiftCards;

public static class GiftCardMapper
{
    public static GiftCardResponse ToResponse(GiftCard card, string? customerName = null)
        => new(
            card.Id,
            card.Code,
            card.StoreId,
            card.InitialBalance,
            card.Balance,
            card.CardStatus.ToString(),
            card.ExpiryDate,
            card.IssuedToCustomerId,
            customerName,
            card.IssuedInTransactionId,
            card.Notes,
            card.CreatedAt,
            card.Transactions.Select(t => new GiftCardTransactionResponse(
                t.Id, t.TransactionType.ToString(), t.Amount, t.BalanceAfter,
                t.SaleTransactionId, t.Notes, t.CreatedAt)).ToList().AsReadOnly());

    public static async Task<GiftCardResponse> ToResponseAsync(
        IUnitOfWork uow, GiftCard card, CancellationToken ct)
    {
        var customer = card.IssuedToCustomerId is Guid id
            ? await uow.Customers.GetByIdAsync(id, ct)
            : null;
        return ToResponse(card, customer?.Name);
    }

    public static GiftCardSummaryResponse ToSummary(GiftCard card, string? customerName = null)
        => new(card.Id, card.Code, card.StoreId, card.IssuedToCustomerId, customerName,
            card.Balance, card.CardStatus.ToString(), card.ExpiryDate, card.CreatedAt);
}
