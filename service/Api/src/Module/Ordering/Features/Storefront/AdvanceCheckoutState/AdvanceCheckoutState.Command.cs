using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Storefront.AdvanceCheckoutState;

public sealed record AdvanceCheckoutStateCommand : ICommand
{
    public Guid CartId { get; init; }
    public CheckoutState TargetState { get; init; }
    /// <summary>The selected payment method, recorded on the cart when advancing to PickPaymentMethod.</summary>
    public Guid? PaymentMethodId { get; init; }
}
