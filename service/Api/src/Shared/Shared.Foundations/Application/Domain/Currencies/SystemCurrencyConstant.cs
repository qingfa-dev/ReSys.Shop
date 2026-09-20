namespace Shared.Application.Domain.Currencies;

public static class SystemCurrencyConstant
{
    public static class Constraints
    {
        public const int MaxCodeLength = 3;
        public const int MonetaryPrecision = 18;
        public const int MonetaryScale = 2;
    }

    public static class Defaults
    {
        public const string Code = "USD";
        public const string Symbol = "$";
        public const string Name = "US Dollar";
        public const int NumericCode = 840;
        public const string Language = "en";
    }
}
