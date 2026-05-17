using BasicCommerce.Contracts.GiftCards;
using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Application.Features.GiftCards;

public static class GiftCardMapper
{
    public static GiftCardResponse ToResponse(GiftCard card)
        => new(
            card.Id,
            card.Code,
            card.StoreId,
            card.InitialBalance,
            card.Balance,
            card.CardStatus.ToString(),
            card.ExpiryDate,
            card.IssuedToCustomerId,
            card.IssuedInTransactionId,
            card.Notes,
            card.CreatedAt,
            card.Transactions.Select(t => new GiftCardTransactionResponse(
                t.Id, t.TransactionType.ToString(), t.Amount, t.BalanceAfter,
                t.SaleTransactionId, t.Notes, t.CreatedAt)).ToList().AsReadOnly());

    public static GiftCardSummaryResponse ToSummary(GiftCard card)
        => new(card.Id, card.Code, card.StoreId, card.Balance,
            card.CardStatus.ToString(), card.ExpiryDate, card.CreatedAt);
}
