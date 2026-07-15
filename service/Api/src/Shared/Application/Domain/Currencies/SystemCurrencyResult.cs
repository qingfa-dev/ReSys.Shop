namespace Shared.Application.Domain.Currencies;

public static class SystemCurrencyResult
{
    public static Error CurrencyInvalid => Error.Validation(
        code: "System.Currency.Invalid",
        message: "Currency must be a valid ISO code.");

    public static Error CurrencyTooLong => Error.Validation(
        code: "System.Currency.TooLong",
        message: $"Currency code must be at most {SystemCurrencyConstant.Constraints.MaxCodeLength} characters.");

    public static Error CurrencyNotSupported => Error.Validation(
        code: "System.Currency.NotSupported",
        message: "Currency code is not in the supported list.");
}
