namespace Module.Ordering.Features.Storefront.Cart.UpdateCheckout;

public static partial class UpdateCheckout
{
    public class Request
    {
        public string? Email { get; init; }
        public Guid? BillAddressId { get; init; }
        public Guid? ShipAddressId { get; init; }
        public string? SpecialInstructions { get; init; }
    }
}
