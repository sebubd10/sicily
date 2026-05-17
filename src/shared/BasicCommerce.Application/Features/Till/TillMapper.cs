using BasicCommerce.Contracts.Till;
using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Application.Features.Till;

public static class TillMapper
{
    public static TillSessionResponse ToResponse(TillSession session)
        => new(
            session.Id,
            session.StoreId,
            session.TerminalId,
            session.OpenedBy,
            session.ClosedBy,
            session.OpeningFloat,
            session.ClosingBalance,
            session.ClosingVariance,
            session.ExpectedClosingBalance,
            session.SessionStatus.ToString(),
            session.OpenedAt,
            session.ClosedAt,
            session.Notes,
            session.PettyTransactions.Select(p => new PettyTransactionResponse(
                p.Id, p.Type.ToString(), p.Amount, p.Reason, p.PerformedBy, p.CreatedAt
            )).ToList().AsReadOnly());

    public static TillSessionSummaryResponse ToSummary(TillSession session)
        => new(session.Id, session.StoreId, session.TerminalId,
            session.OpenedBy, session.OpeningFloat,
            session.SessionStatus.ToString(), session.OpenedAt, session.ClosedAt);

    public static TillReportResponse ToReport(TillSessionSnapshot snap, decimal? actualCash)
        => new(snap.SessionId, snap.OpeningFloat,
            snap.CashSalesTotal, snap.CardSalesTotal,
            snap.GiftCardSalesTotal, snap.TotalRefunds,
            snap.PettyCashIn, snap.PettyCashOut, snap.ExpectedCash,
            actualCash,
            actualCash.HasValue ? actualCash.Value - snap.ExpectedCash : null,
            snap.IsFinal);
}
