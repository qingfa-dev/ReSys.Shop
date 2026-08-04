using Shared.Application.Domain.Models;

namespace Module.Shipping.Domain.ShippingMethods;

/// <summary>Associates a shipping method with a geographic zone.</summary>
public sealed class ShippingMethodZone : Entity
{
    public Guid ShippingMethodId { get; set; }
    /// <summary>ISO 3166-1 alpha-2 country code, or "*" for all countries.</summary>
    public string CountryCode { get; set; } = string.Empty;
    /// <summary>Optional ISO 3166-2 subdivision code within the country.</summary>
    public string? StateCode { get; set; }
}
