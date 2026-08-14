using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.AdvanceCheckoutState;

public sealed record AdvanceCheckoutStateCommand : ICommand
{
    public Guid CartId { get; init; }
    public CheckoutState TargetState { get; init; }
}
