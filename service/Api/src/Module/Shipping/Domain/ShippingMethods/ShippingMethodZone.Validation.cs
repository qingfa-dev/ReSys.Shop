using System.Text.RegularExpressions;

namespace Module.Shipping.Domain.ShippingMethods;

public static partial class ShippingMethodZoneValidation
{
    // ISO 3166-1 alpha-2 uppercase letters, or the wildcard '*'.
    [GeneratedRegex(@"^([A-Z]{2}|\*)$")]
    private static partial Regex CountryCodePattern();

    public static IRuleBuilderOptions<T, string?> ApplyCountryCodeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(ShippingMethodZoneResult.Errors.CountryCodeRequired.Code)
            .WithMessage(ShippingMethodZoneResult.Errors.CountryCodeRequired.Message)
            .Matches(CountryCodePattern())
            .WithErrorCode(ShippingMethodZoneResult.Errors.CountryCodeInvalid.Code)
            .WithMessage(ShippingMethodZoneResult.Errors.CountryCodeInvalid.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyStateCodeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(ShippingMethodZoneConstant.Constraints.MaxStateCodeLength)
            .WithErrorCode(ShippingMethodZoneResult.Errors.StateCodeTooLong.Code)
            .WithMessage(ShippingMethodZoneResult.Errors.StateCodeTooLong.Message);
    }
}