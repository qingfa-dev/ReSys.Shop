namespace Shared.Application.Domain.Currencies;

public sealed record SystemCurrency
{
    public string Code { get; init; } = default!;
    public string Symbol { get; init; } = default!;
    public string Name { get; init; } = default!;
    public int NumericCode { get; init; }

    private static readonly Dictionary<string, SystemCurrency> _supported = new(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = new() { Code = "USD", Symbol = "$", Name = "US Dollar", NumericCode = 840 },
        ["EUR"] = new() { Code = "EUR", Symbol = "€", Name = "Euro", NumericCode = 978 },
        ["GBP"] = new() { Code = "GBP", Symbol = "£", Name = "British Pound", NumericCode = 826 },
    };

    public static IReadOnlyDictionary<string, SystemCurrency> Supported => _supported;

    public static SystemCurrency Default => _supported[SystemCurrencyConstant.Defaults.Code];

    public static bool IsSupported(string code) => !string.IsNullOrEmpty(code) && _supported.ContainsKey(code);
}
