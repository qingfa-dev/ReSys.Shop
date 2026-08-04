namespace Module.Ordering.Features.Admin.Orders.UpdateShipmentState;

public static partial class UpdateOrderShipmentState
{
    public sealed record Request
    {
        public string ShipmentState { get; init; } = string.Empty;
    }
}
