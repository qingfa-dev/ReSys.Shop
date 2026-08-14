using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.RegressCheckoutState;

public sealed record RegressCheckoutStateCommand : ICommand
{
    public Guid CartId { get; init; }
    public CheckoutState TargetState { get; init; }
}
