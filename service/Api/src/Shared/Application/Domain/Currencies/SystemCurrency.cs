namespace Shared.Application.Domain.Currencies;

public sealed record SystemCurrency(string Code, string Symbol, string Name, int NumericCode)
{
    private static readonly Dictionary<string, SystemCurrency> _supported = new(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = new("USD", "$", "US Dollar", 840),
        ["EUR"] = new("EUR", "€", "Euro", 978),
        ["GBP"] = new("GBP", "£", "British Pound", 826),
    };

    public static IReadOnlyDictionary<string, SystemCurrency> Supported => _supported;

    public static SystemCurrency Default => _supported[SystemCurrencyConstant.Defaults.Code];

    public static bool IsSupported(string code) => !string.IsNullOrEmpty(code) && _supported.ContainsKey(code);
}
