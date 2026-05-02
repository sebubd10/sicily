namespace BasicCommerce.Domain.Events;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public record TransactionOpenedEvent(
    Guid TransactionId,
    Guid StoreId,
    Guid TerminalId) : DomainEvent;

public record TransactionCompletedEvent(
    Guid TransactionId,
    Guid StoreId,
    decimal Total,
    decimal TaxTotal) : DomainEvent;

public record TransactionVoidedEvent(
    Guid TransactionId,
    Guid StoreId,
    Guid VoidedBy) : DomainEvent;

public record StockLowEvent(
    Guid ProductId,
    Guid StoreId,
    decimal CurrentQuantity,
    decimal Threshold) : DomainEvent;

public record UserLockedEvent(
    Guid UserId,
    Guid TenantId,
    string Email) : DomainEvent;
