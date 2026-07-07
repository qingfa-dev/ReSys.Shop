using Module.Shipping.Features.Admin.Shipments.Shared.Models;

namespace Module.Shipping.Features.Admin.Shipments.Ship;
public static partial class MarkShipmentShipped
{
    public class Request : ShipmentRequest { public new string Tracking { get; init; } = string.Empty; }
}
