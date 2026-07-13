namespace Module.Ordering.Features.Admin.Orders.UpdateShipAddress;

public static partial class UpdateOrderShipAddress
{
    public sealed record Request
    {
        public Guid AddressId { get; init; }
    }
}