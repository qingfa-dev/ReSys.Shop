namespace Module.Ordering.Features.Admin.Orders.Shared.Models;

/// <summary>Parameters for actions that target an order address by identifier.</summary>
public abstract record OrderAddressActionParameters
{
    /// <summary>The address to act on.</summary>
    public Guid AddressId { get; init; }
}
