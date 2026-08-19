namespace Module.Shipping.Domain.ShippingMethods;

public static class ShippingMethodZoneConstant
{
    public static class Constraints
    {
        public const int MaxCountryCodeLength = 2;
        public const int MaxStateCodeLength = 10;
        public const string WildcardCountryCode = "*";
    }
}