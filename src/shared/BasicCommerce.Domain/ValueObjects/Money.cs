namespace BasicCommerce.Domain.ValueObjects;

public sealed record Money(decimal Amount, string Currency = "BDT")
{
    public static Money Zero(string currency = "BDT") => new(0, currency);
    public static Money Of(decimal amount, string currency = "BDT") => new(amount, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);

    public Money ApplyVat(decimal vatRate) => new(Amount * (1 + vatRate / 100), Currency);

    public override string ToString() => $"{Currency} {Amount:N2}";

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot operate on different currencies: {Currency} and {other.Currency}.");
    }
}
