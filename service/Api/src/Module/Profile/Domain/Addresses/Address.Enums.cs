using System.Text.Json.Serialization;

namespace Module.Profile.Domain.Addresses;

/// <summary>
/// Specifies the type of an address.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AddressType
{
    /// <summary>
    /// A shipping address where products are delivered.
    /// </summary>
    Shipping = 1,

    /// <summary>
    /// A billing address associated with a payment method.
    /// </summary>
    Billing = 2,

    /// <summary>
    /// Any other type of address (e.g., office, pickup point).
    /// </summary>
    Other = 3
}