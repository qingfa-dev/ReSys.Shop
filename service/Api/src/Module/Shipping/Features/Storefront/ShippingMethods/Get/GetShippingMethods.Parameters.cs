namespace Module.Shipping.Features.Storefront.ShippingMethods.Get;

public static partial class GetShippingMethods
{
    public sealed record Parameters : QueryingParameters
    {
        /// <summary>Optional ISO 3166-1 alpha-2 country code to filter methods by delivery zone.</summary>
        [FromQuery(Name = "countryCode")]
        public string? CountryCode { get; init; }
    }
}
