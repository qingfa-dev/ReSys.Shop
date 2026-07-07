using Module.Shipping.Domain.Shipments;
using Module.Shipping.Features.Admin.Shipments.Shared.Models;

namespace Module.Shipping.Features.Admin.Shipments.Shared.Mappings;

public static partial class ShipmentMapping
{
    /// <summary>Maps a request to a new Shipment domain entity.</summary>
    public static Result<Shipment> MapToDomain<T>(this T request) where T : ShipmentRequest
    {
        return ShipmentExtensions.Create(
            orderId: request.OrderId,
            stockLocationId: request.StockLocationId);
    }
}
