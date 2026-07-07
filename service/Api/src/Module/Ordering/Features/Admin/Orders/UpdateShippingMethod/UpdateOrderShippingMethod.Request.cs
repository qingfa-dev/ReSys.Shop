namespace Module.Ordering.Features.Admin.Orders.UpdateShippingMethod;

public static partial class UpdateOrderShippingMethod
{
    public class Request
    {
        public Guid ShippingMethodId { get; init; }
    }
}
