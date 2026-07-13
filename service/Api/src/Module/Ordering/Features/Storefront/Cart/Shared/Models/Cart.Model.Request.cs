namespace Module.Ordering.Features.Storefront.Cart.Shared.Models;

/// <summary>Cart request DTO — inherits cart parameters (variant ID, quantity, notes) for input validation.</summary>
public record CartRequest : CartParameters;