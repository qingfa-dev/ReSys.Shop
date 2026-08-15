namespace Module.Ordering.Features.Admin.Orders.Shared.Models;

/// <summary>Parameters for actions that select a shipping method by identifier.</summary>
public abstract record ShippingMethodActionParameters
{
    /// <summary>The shipping method to select.</summary>
    public Guid ShippingMethodId { get; init; }
}
