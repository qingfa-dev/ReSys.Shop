namespace Module.Ordering.Features.Storefront.Cart.Shared.Models;

/// <summary>Base parameters for cart operations: identifies a variant, quantity, and optional notes.</summary>
public abstract class CartParameters
{
    /// <summary>Variant (SKU) identifier to add or update.</summary>
    public Guid VariantId { get; init; }
    /// <summary>Quantity of the variant (defaults to 1).</summary>
    public int Quantity { get; init; } = 1;
    /// <summary>Optional notes attached to the cart item.</summary>
    public string? Notes { get; init; }
}
