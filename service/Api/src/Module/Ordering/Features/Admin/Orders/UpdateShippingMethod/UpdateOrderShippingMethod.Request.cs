namespace Module.Ordering.Features.Admin.Orders.UpdateShippingMethod;

public static partial class UpdateOrderShippingMethod
{
    public record Request
    {
        public Guid ShippingMethodId { get; init; }
    }
}
