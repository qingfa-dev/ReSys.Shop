namespace Module.Ordering.Features.Admin.Orders.Shared.Models;

/// <summary>Parameters for actions that set a line item quantity.</summary>
public abstract record LineItemQuantityParameters
{
    /// <summary>The new quantity for the line item.</summary>
    public int Quantity { get; init; }
}
