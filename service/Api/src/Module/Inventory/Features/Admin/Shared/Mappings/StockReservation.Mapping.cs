using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Admin.Shared.Models;

namespace Module.Inventory.Features.Admin.Shared.Mappings;

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

public static partial class StockReservationMapping
{
    // Map: Domain entity -> Detail response
    public static T MapToDetail<T>(this StockReservation entity) where T : StockReservationDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            VariantId = entity.VariantId,
            StockLocationId = entity.StockLocationId,
            OrderId = entity.OrderId,
            Quantity = entity.Quantity,
            State = entity.State,
            ExpiresAtUtc = entity.ExpiresAtUtc,
            Reason = entity.Reason ?? string.Empty,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
        };
    }

    // Map: Domain entity -> List item response
    public static T MapToListItem<T>(this StockReservation entity) where T : StockReservationListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            VariantId = entity.VariantId,
            StockLocationId = entity.StockLocationId,
            OrderId = entity.OrderId,
            Quantity = entity.Quantity,
            State = entity.State,
            ExpiresAtUtc = entity.ExpiresAtUtc,
            Reason = entity.Reason ?? string.Empty,
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }
}
