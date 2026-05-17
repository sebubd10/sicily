using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;

namespace BasicCommerce.Domain.Entities;

/// <summary>
/// Tracks credit/due balances for customers — common practice in Bangladeshi retail.
/// </summary>
public class CreditAccount : TenantEntity
{
    public Guid CustomerId { get; private set; }
    public Guid StoreId { get; private set; }
    public decimal CreditLimit { get; private set; }
    public decimal OutstandingBalance { get; private set; }
    public DateTime? LastPaymentAt { get; private set; }

    public decimal AvailableCredit => CreditLimit - OutstandingBalance;
    public bool CanPurchase(decimal amount) => AvailableCredit >= amount;

    private readonly List<CreditTransaction> _transactions = [];
    public IReadOnlyCollection<CreditTransaction> Transactions => _transactions.AsReadOnly();

    private CreditAccount() { }

    public static CreditAccount Create(Guid tenantId, Guid customerId, Guid storeId,
        decimal creditLimit)
    {
        return new CreditAccount
        {
            TenantId = tenantId,
            CustomerId = customerId,
            StoreId = storeId,
            CreditLimit = creditLimit
        };
    }

    public void Charge(Guid transactionId, decimal amount, string description)
    {
        if (!CanPurchase(amount))
            throw new DomainException($"Credit limit exceeded. Available: {AvailableCredit:C}.");

        OutstandingBalance += amount;
        _transactions.Add(CreditTransaction.CreateCharge(Id, transactionId, amount, description));
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordPayment(decimal amount, string? reference = null)
    {
        if (amount <= 0)
            throw new DomainException("Payment amount must be greater than zero.");

        OutstandingBalance = Math.Max(0, OutstandingBalance - amount);
        LastPaymentAt = DateTime.UtcNow;
        _transactions.Add(CreditTransaction.CreatePayment(Id, amount, reference));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCreditLimit(decimal newLimit) => CreditLimit = newLimit;
}

public class CreditTransaction : BaseEntity
{
    public Guid CreditAccountId { get; private set; }
    public Guid? TransactionId { get; private set; }
    public decimal Amount { get; private set; }
    public bool IsCharge { get; private set; }
    public string Description { get; private set; } = default!;
    public string? Reference { get; private set; }

    private CreditTransaction() { }

    internal static CreditTransaction CreateCharge(Guid creditAccountId,
        Guid transactionId, decimal amount, string description) =>
        new()
        {
            CreditAccountId = creditAccountId,
            TransactionId = transactionId,
            Amount = amount,
            IsCharge = true,
            Description = description
        };

    internal static CreditTransaction CreatePayment(Guid creditAccountId,
        decimal amount, string? reference = null) =>
        new()
        {
            CreditAccountId = creditAccountId,
            Amount = amount,
            IsCharge = false,
            Description = "Payment received",
            Reference = reference
        };
}
