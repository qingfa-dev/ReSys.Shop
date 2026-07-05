using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Features.Admin.StockReservations.Shared.Models;

namespace Module.Inventory.Features.Admin.StockReservations.Shared.Mappings;

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
