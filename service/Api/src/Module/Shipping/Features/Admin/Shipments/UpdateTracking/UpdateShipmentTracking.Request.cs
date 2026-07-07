using Module.Shipping.Features.Admin.Shipments.Shared.Models;

namespace Module.Shipping.Features.Admin.Shipments.UpdateTracking;
public static partial class UpdateShipmentTracking
{
    public class Request : ShipmentRequest { public new string Tracking { get; init; } = string.Empty; }
}
