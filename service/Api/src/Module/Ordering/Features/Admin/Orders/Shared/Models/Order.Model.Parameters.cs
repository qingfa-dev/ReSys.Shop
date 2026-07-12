using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.Shared.Models;

/// <summary>Base parameters shared across order requests — currency defaults to system default; all fields optional for PATCH semantics.</summary>
// Invariant: Currency defaults to system default; address IDs nullable until assigned
public abstract class OrderParameters
{
    public string Currency { get; init; } = OrderConstant.Defaults.Currency;
    public string? Email { get; init; }
    public string? SpecialInstructions { get; init; }
    public Guid? BillAddressId { get; init; }
    public Guid? ShipAddressId { get; init; }
    public Guid? ShippingMethodId { get; init; }
}
