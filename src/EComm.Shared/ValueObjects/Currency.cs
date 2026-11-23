namespace EComm.Shared.ValueObjects;

public sealed record Currency
{
    public static readonly Currency Try = new("TRY", "₺");
    public static readonly Currency Usd = new("USD", "$");
    public static readonly Currency Eur = new("EUR", "€");

    private static readonly Dictionary<string, Currency> ByCode =
        new[] { Try, Usd, Eur }.ToDictionary(c => c.Code);

    public string Code { get; }
    public string Symbol { get; }

    private Currency(string code, string symbol)
    {
        Code = code;
        Symbol = symbol;
    }
    public static Currency FromCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var normalized = code.Trim().ToUpperInvariant();

        return ByCode.TryGetValue(normalized, out var currency)
            ? currency
            : throw new ArgumentException($"Desteklenmeyen para birimi: {code}", nameof(code));
    }

}