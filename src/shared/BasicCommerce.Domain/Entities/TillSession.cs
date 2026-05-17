using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;

namespace BasicCommerce.Domain.Entities;

public class TillSession : TenantEntity
{
    public Guid StoreId { get; private set; }
    public Guid TerminalId { get; private set; }
    public Guid OpenedBy { get; private set; }
    public Guid? ClosedBy { get; private set; }
    public decimal OpeningFloat { get; private set; }
    public decimal? ClosingBalance { get; private set; }
    public decimal? ClosingVariance { get; private set; }
    public decimal? ExpectedClosingBalance { get; private set; }
    public TillSessionStatus SessionStatus { get; private set; } = TillSessionStatus.Open;
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<PettyTransaction> _pettyTransactions = [];
    public IReadOnlyCollection<PettyTransaction> PettyTransactions => _pettyTransactions.AsReadOnly();

    private TillSession() { }

    public static TillSession Open(Guid tenantId, Guid storeId, Guid terminalId,
        Guid openedBy, decimal openingFloat, string? notes = null)
    {
        if (openingFloat < 0)
            throw new DomainException("Opening float cannot be negative.");

        return new TillSession
        {
            TenantId = tenantId,
            StoreId = storeId,
            TerminalId = terminalId,
            OpenedBy = openedBy,
            OpeningFloat = openingFloat,
            OpenedAt = DateTime.UtcNow,
            Notes = notes
        };
    }

    public PettyTransaction AddPettyCash(PettyTransactionType type, decimal amount,
        string reason, Guid performedBy)
    {
        EnsureOpen();
        if (amount <= 0)
            throw new DomainException("Petty cash amount must be greater than zero.");

        var tx = PettyTransaction.Create(Id, type, amount, reason, performedBy);
        _pettyTransactions.Add(tx);
        UpdatedAt = DateTime.UtcNow;
        return tx;
    }

    public TillSessionSnapshot TakeXReport(decimal cashSalesTotal, decimal cardSalesTotal,
        decimal giftCardSalesTotal, decimal totalRefunds)
    {
        EnsureOpen();
        var pettyCashIn = _pettyTransactions
            .Where(p => p.Type == PettyTransactionType.CashIn)
            .Sum(p => p.Amount);
        var pettyCashOut = _pettyTransactions
            .Where(p => p.Type == PettyTransactionType.CashOut)
            .Sum(p => p.Amount);

        return new TillSessionSnapshot(
            SessionId: Id,
            OpeningFloat: OpeningFloat,
            CashSalesTotal: cashSalesTotal,
            CardSalesTotal: cardSalesTotal,
            GiftCardSalesTotal: giftCardSalesTotal,
            TotalRefunds: totalRefunds,
            PettyCashIn: pettyCashIn,
            PettyCashOut: pettyCashOut,
            ExpectedCash: OpeningFloat + cashSalesTotal - totalRefunds + pettyCashIn - pettyCashOut,
            IsFinal: false
        );
    }

    public TillSessionSnapshot Close(Guid closedBy, decimal closingBalance,
        decimal cashSalesTotal, decimal cardSalesTotal,
        decimal giftCardSalesTotal, decimal totalRefunds, string? notes = null)
    {
        EnsureOpen();

        var pettyCashIn = _pettyTransactions
            .Where(p => p.Type == PettyTransactionType.CashIn)
            .Sum(p => p.Amount);
        var pettyCashOut = _pettyTransactions
            .Where(p => p.Type == PettyTransactionType.CashOut)
            .Sum(p => p.Amount);

        var expected = OpeningFloat + cashSalesTotal - totalRefunds + pettyCashIn - pettyCashOut;

        ClosedBy = closedBy;
        ClosingBalance = closingBalance;
        ExpectedClosingBalance = expected;
        ClosingVariance = closingBalance - expected;
        SessionStatus = TillSessionStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        Notes = notes ?? Notes;
        UpdatedAt = DateTime.UtcNow;

        return new TillSessionSnapshot(
            SessionId: Id,
            OpeningFloat: OpeningFloat,
            CashSalesTotal: cashSalesTotal,
            CardSalesTotal: cardSalesTotal,
            GiftCardSalesTotal: giftCardSalesTotal,
            TotalRefunds: totalRefunds,
            PettyCashIn: pettyCashIn,
            PettyCashOut: pettyCashOut,
            ExpectedCash: expected,
            IsFinal: true
        );
    }

    private void EnsureOpen()
    {
        if (SessionStatus != TillSessionStatus.Open)
            throw new DomainException("Till session is not open.");
    }
}

public record TillSessionSnapshot(
    Guid SessionId,
    decimal OpeningFloat,
    decimal CashSalesTotal,
    decimal CardSalesTotal,
    decimal GiftCardSalesTotal,
    decimal TotalRefunds,
    decimal PettyCashIn,
    decimal PettyCashOut,
    decimal ExpectedCash,
    bool IsFinal);

public class PettyTransaction : BaseEntity
{
    public Guid TillSessionId { get; private set; }
    public PettyTransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Reason { get; private set; } = default!;
    public Guid PerformedBy { get; private set; }

    private PettyTransaction() { }

    internal static PettyTransaction Create(Guid sessionId, PettyTransactionType type,
        decimal amount, string reason, Guid performedBy)
        => new()
        {
            TillSessionId = sessionId,
            Type = type,
            Amount = amount,
            Reason = reason,
            PerformedBy = performedBy
        };
}
