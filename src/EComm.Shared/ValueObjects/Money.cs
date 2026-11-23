namespace EComm.Shared.ValueObjects;

public sealed record Money
{
    private const int DecimalPlaces = 2;
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);
        if (amount < 0)
            throw new ArgumentException("Tutar negatif olamaz.", nameof(amount));

        Amount = Math.Round(amount, DecimalPlaces, MidpointRounding.AwayFromZero);
        Currency = currency;
    }
    public static Money Zero(Currency currency) => new(0, currency);
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);

        return new Money(Amount + other.Amount, Currency);
    }
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);

        if (Amount < other.Amount)
            throw new InvalidOperationException(
                $"Yetersiz tutar: {Amount} {Currency.Code} tutarından {other.Amount} {Currency.Code} çıkarılamaz.");

        return new Money(Amount - other.Amount, Currency);
    }
    public Money Multiply(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Adet negatif olamaz.", nameof(quantity));

        return new Money(Amount * quantity, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"{Currency.Code} ile {other.Currency.Code} para birimleri arasında işlem yapılamaz.");
    }
}