namespace Module.Ordering.Features.Admin.Orders.UpdateBillAddress;

public static partial class UpdateOrderBillAddress
{
    public record Request
    {
        public Guid AddressId { get; init; }
    }
}
