using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Events;
using BasicCommerce.Domain.Exceptions;
using BasicCommerce.Domain.ValueObjects;

namespace BasicCommerce.Domain.Entities;

public class Transaction : TenantEntity
{
    public string TransactionNumber { get; private set; } = default!;
    public Guid StoreId { get; private set; }
    public Guid TerminalId { get; private set; }
    public Guid CashierId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public TransactionStatus Status { get; private set; } = TransactionStatus.Open;
    public TransactionType Type { get; private set; } = TransactionType.Sale;
    public Guid? OriginalTransactionId { get; private set; }

    public decimal SubTotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal DiscountTotal { get; private set; }
    public decimal Total { get; private set; }
    public decimal AmountPaid { get; private set; }
    public decimal ChangeDue { get; private set; }

    public string? Notes { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? VoidedAt { get; private set; }
    public Guid? VoidedBy { get; private set; }
    public string? VoidReason { get; private set; }
    public bool IsSynced { get; private set; }

    private readonly List<LineItem> _lineItems = [];
    private readonly List<Payment> _payments = [];
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<LineItem> LineItems => _lineItems.AsReadOnly();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Transaction() { }

    public static Transaction Create(Guid tenantId, Guid storeId, Guid terminalId,
        Guid cashierId, Guid? customerId = null)
    {
        var transaction = new Transaction
        {
            TenantId = tenantId,
            StoreId = storeId,
            TerminalId = terminalId,
            CashierId = cashierId,
            CustomerId = customerId,
            TransactionNumber = GenerateNumber()
        };

        transaction._domainEvents.Add(new TransactionOpenedEvent(transaction.Id, storeId, terminalId));
        return transaction;
    }

    public LineItem AddItem(Product product, decimal quantity, decimal? overridePrice = null,
        Guid? overrideApprovedBy = null)
    {
        if (Status != TransactionStatus.Open)
            throw new DomainException("Cannot add items to a non-open transaction.");

        var unitPrice = overridePrice ?? product.Price.Amount;
        var lineItem = LineItem.Create(Id, product, quantity, unitPrice, overrideApprovedBy);

        _lineItems.Add(lineItem);
        RecalculateTotals();
        return lineItem;
    }

    public void VoidLineItem(Guid lineItemId, Guid cashierId)
    {
        var item = _lineItems.FirstOrDefault(l => l.Id == lineItemId)
            ?? throw new DomainException("Line item not found.");

        item.Void(cashierId);
        RecalculateTotals();
    }

    public Payment AddPayment(PaymentMethod method, decimal amount, string? reference = null)
    {
        if (Status != TransactionStatus.Open)
            throw new DomainException("Cannot add payment to a non-open transaction.");

        var payment = Payment.Create(Id, TenantId, method, amount, reference);
        _payments.Add(payment);
        AmountPaid = _payments.Where(p => p.Status == PaymentStatus.Approved).Sum(p => p.Amount);
        return payment;
    }

    public void Complete()
    {
        if (Total > AmountPaid)
            throw new DomainException("Transaction total exceeds amount paid.");

        ChangeDue = AmountPaid - Total;
        Status = TransactionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        _domainEvents.Add(new TransactionCompletedEvent(Id, StoreId, Total, TaxTotal));
    }

    public void Void(Guid voidedBy, string reason)
    {
        if (Status == TransactionStatus.Voided)
            throw new DomainException("Transaction is already voided.");

        Status = TransactionStatus.Voided;
        VoidedAt = DateTime.UtcNow;
        VoidedBy = voidedBy;
        VoidReason = reason;
        _domainEvents.Add(new TransactionVoidedEvent(Id, StoreId, voidedBy));
    }

    public void MarkSynced() => IsSynced = true;

    private void RecalculateTotals()
    {
        var activeItems = _lineItems.Where(l => !l.IsVoided).ToList();
        SubTotal = activeItems.Sum(l => l.LineTotal);
        TaxTotal = activeItems.Sum(l => l.TaxAmount);
        DiscountTotal = activeItems.Sum(l => l.DiscountAmount);
        Total = SubTotal + TaxTotal - DiscountTotal;
    }

    private static string GenerateNumber() =>
        $"TXN-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";
}
