namespace Module.Shipping.Domain.ShippingMethods;

public static class ShippingMethodZoneResult
{
    public static class Errors
    {
        public static Error CountryCodeRequired => Error.Validation(
            code: "ShippingMethodZone.CountryCode.Required",
            message: "Country code is required.");

        public static Error CountryCodeInvalid => Error.Validation(
            code: "ShippingMethodZone.CountryCode.Invalid",
            message: $"Country code must be a valid ISO 3166-1 alpha-2 code or '{ShippingMethodZoneConstant.Constraints.WildcardCountryCode}'.");

        public static Error StateCodeTooLong => Error.Validation(
            code: "ShippingMethodZone.StateCode.TooLong",
            message: $"State code cannot exceed {ShippingMethodZoneConstant.Constraints.MaxStateCodeLength} characters.");
    }
}