using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;

namespace BasicCommerce.Domain.Entities;

public class GiftCard : TenantEntity
{
    public string Code { get; private set; } = default!;
    public Guid StoreId { get; private set; }
    public decimal InitialBalance { get; private set; }
    public decimal Balance { get; private set; }
    public GiftCardStatus CardStatus { get; private set; } = GiftCardStatus.Inactive;
    public DateTime? ExpiryDate { get; private set; }
    public Guid? IssuedToCustomerId { get; private set; }
    public Guid? IssuedInTransactionId { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<GiftCardTransaction> _transactions = [];
    public IReadOnlyCollection<GiftCardTransaction> Transactions => _transactions.AsReadOnly();

    private GiftCard() { }

    public static GiftCard Issue(Guid tenantId, Guid storeId, string code, decimal amount,
        DateTime? expiryDate, Guid? issuedToCustomerId, Guid? issuedInTransactionId, string? notes = null)
    {
        if (amount <= 0)
            throw new DomainException("Gift card amount must be greater than zero.");

        var card = new GiftCard
        {
            TenantId = tenantId,
            StoreId = storeId,
            Code = code.Trim().ToUpperInvariant(),
            InitialBalance = amount,
            Balance = amount,
            CardStatus = GiftCardStatus.Active,
            ExpiryDate = expiryDate,
            IssuedToCustomerId = issuedToCustomerId,
            IssuedInTransactionId = issuedInTransactionId,
            Notes = notes
        };

        card._transactions.Add(GiftCardTransaction.Create(
            card.Id, GiftCardTransactionType.Issue, amount, amount, null, "Gift card issued"));

        return card;
    }

    public decimal Redeem(decimal amount, Guid transactionId)
    {
        EnsureActive();
        if (amount <= 0)
            throw new DomainException("Redemption amount must be greater than zero.");

        var actual = Math.Min(amount, Balance);
        Balance -= actual;

        if (Balance == 0)
            CardStatus = GiftCardStatus.Depleted;

        _transactions.Add(GiftCardTransaction.Create(
            Id, GiftCardTransactionType.Redeem, -actual, Balance, transactionId, null));

        UpdatedAt = DateTime.UtcNow;
        return actual;
    }

    public void Reload(decimal amount, string? notes = null)
    {
        if (CardStatus == GiftCardStatus.Cancelled || CardStatus == GiftCardStatus.Expired)
            throw new DomainException($"Cannot reload a {CardStatus} gift card.");

        if (amount <= 0)
            throw new DomainException("Reload amount must be greater than zero.");

        Balance += amount;

        if (CardStatus == GiftCardStatus.Depleted)
            CardStatus = GiftCardStatus.Active;

        _transactions.Add(GiftCardTransaction.Create(
            Id, GiftCardTransactionType.Reload, amount, Balance, null, notes ?? "Gift card reloaded"));

        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (CardStatus != GiftCardStatus.Inactive)
            throw new DomainException("Card is not in Inactive status.");
        CardStatus = GiftCardStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string? reason = null)
    {
        if (CardStatus == GiftCardStatus.Cancelled)
            throw new DomainException("Gift card is already cancelled.");

        CardStatus = GiftCardStatus.Cancelled;
        _transactions.Add(GiftCardTransaction.Create(
            Id, GiftCardTransactionType.Cancel, 0, Balance, null, reason ?? "Cancelled"));
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkExpired()
    {
        if (CardStatus == GiftCardStatus.Cancelled)
            throw new DomainException("Cannot expire a cancelled gift card.");

        CardStatus = GiftCardStatus.Expired;
        _transactions.Add(GiftCardTransaction.Create(
            Id, GiftCardTransactionType.Expire, 0, Balance, null, "Gift card expired"));
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureActive()
    {
        if (CardStatus != GiftCardStatus.Active)
            throw new DomainException($"Gift card is not active. Current status: {CardStatus}.");

        if (ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow.Date)
        {
            MarkExpired();
            throw new DomainException("Gift card has expired.");
        }
    }
}

public class GiftCardTransaction : BaseEntity
{
    public Guid GiftCardId { get; private set; }
    public GiftCardTransactionType TransactionType { get; private set; }
    public decimal Amount { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public Guid? SaleTransactionId { get; private set; }
    public string? Notes { get; private set; }

    private GiftCardTransaction() { }

    internal static GiftCardTransaction Create(Guid giftCardId, GiftCardTransactionType type,
        decimal amount, decimal balanceAfter, Guid? saleTransactionId, string? notes)
        => new()
        {
            GiftCardId = giftCardId,
            TransactionType = type,
            Amount = amount,
            BalanceAfter = balanceAfter,
            SaleTransactionId = saleTransactionId,
            Notes = notes
        };
}
