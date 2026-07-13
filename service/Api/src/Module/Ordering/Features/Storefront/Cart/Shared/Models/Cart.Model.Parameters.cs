namespace Module.Ordering.Features.Storefront.Cart.Shared.Models;

/// <summary>Input parameters for cart operations — identifies the variant, quantity, and optional notes.</summary>
public abstract record CartParameters
{
    /// <summary>The product variant to add to the cart.</summary>
    public Guid VariantId { get; init; }
    /// <summary>Number of units to add; defaults to 1.</summary>
    public int Quantity { get; init; } = 1;
    /// <summary>Optional notes attached to the cart item (e.g. gift wrap request).</summary>
    public string? Notes { get; init; }
}