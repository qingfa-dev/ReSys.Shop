using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.Shared.Models;

namespace Module.Shipping.Features.Admin.Shipments.Ship;
public static partial class MarkShipmentShipped
{
    public class Response : ShipmentDetailResponse
    {
        public ShipmentState State { get; init; }
    }
}
