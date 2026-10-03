using Module.Ordering.Domain.Orders;
using Module.Shipping.Domain.Shipments;

using Shared.Application.Domain.Models;

namespace Module.Customer.Domain.Addresses;

// Invariant: FirstName != null; Address1 != null; City != null; CountryName != null; IsDefault implies valid address type
public sealed partial class Address : Entity
{
    #region Properties

    public AddressType AddressType { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string Address1 { get; set; } = string.Empty;
    public string? Address2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? ZipCode { get; set; }
    public string? Phone { get; set; }
    public string? Label { get; set; }
    public bool IsDefault { get; set; }
    public bool IsDefaultBilling { get; set; }
    public bool IsDefaultShipping { get; set; }

    public string CountryName { get; set; } = string.Empty;
    public string? StateProvince { get; set; }
    public string? CountryCode { get; set; }
    public string? StateCode { get; set; }

    #endregion

    #region Relationships

    public Guid? UserProfileId { get; set; }
    public UserProfile? UserProfile { get; set; }

    public ICollection<Order> BillingOrders { get; set; } = [];
    public ICollection<Order> ShippingOrders { get; set; } = [];
    public ICollection<Shipment> Shipments { get; set; } = [];

    #endregion
}