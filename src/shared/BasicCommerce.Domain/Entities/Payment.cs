using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Domain.Entities;

public class Payment : TenantEntity
{
    public Guid TransactionId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;
    public string? GatewayReference { get; private set; }
    public string? GatewayResponse { get; private set; }
    public string? MobileNumber { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? DeclineReason { get; private set; }

    private Payment() { }

    internal static Payment Create(Guid transactionId, Guid tenantId,
        PaymentMethod method, decimal amount, string? reference = null)
    {
        return new Payment
        {
            TenantId = tenantId,
            TransactionId = transactionId,
            Method = method,
            Amount = amount,
            GatewayReference = reference
        };
    }

    public void Approve(string? gatewayRef = null)
    {
        PaymentStatus = PaymentStatus.Approved;
        GatewayReference = gatewayRef ?? GatewayReference;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Decline(string reason)
    {
        PaymentStatus = PaymentStatus.Declined;
        DeclineReason = reason;
        ProcessedAt = DateTime.UtcNow;
    }

    public void SetMobileNumber(string mobileNumber) => MobileNumber = mobileNumber;
}
