using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Admin.StockReservations.Shared.Models;

namespace Module.Inventory.Features.Admin.StockReservations.Shared.Mappings;

public static partial class StockReservationMapping
{
    // Map: Request -> Domain entity (create/reserve)
    public static Result<StockReservation> MapToDomain<T>(this T request, int ttlMinutes = StockReservationConstant.Defaults.DefaultTtlMinutes)
        where T : StockReservationRequest
    {
        return StockReservationMethod.Reserve(
            variantId: request.VariantId,
            quantity: request.Quantity,
            stockLocationId: request.StockLocationId,
            orderId: request.OrderId,
            ttlMinutes: ttlMinutes);
    }
}