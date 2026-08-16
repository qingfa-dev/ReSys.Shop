using Module.Shipping.Features.Admin.Shared.Models;

namespace Module.Shipping.Features.Admin.Shipments.UpdateStatus;

public static partial class UpdateShipmentStatus
{
    public sealed record Request : ShipmentStatusParameters;
}
