using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Features.Admin.StockTransfers.Shared.Models;

namespace Module.Inventory.Features.Admin.StockTransfers.Shared.Mappings;

public static partial class StockTransferMapping
{
    public static T MapToDetail<T>(this StockTransfer entity)
        where T : StockTransferDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Number = entity.Number,
            Reference = entity.Reference,
            State = entity.State.ToString(),
            SourceLocationId = entity.SourceLocationId,
            DestinationLocationId = entity.DestinationLocationId,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            Items = entity.TransferItems.Select(ti => new TransferItemResponse
            {
                Id = ti.Id,
                VariantId = ti.VariantId,
                Quantity = ti.Quantity,
                ReceivedQuantity = ti.ReceivedQuantity
            }).ToList()
        };
    }

    public static T MapToListItem<T>(this StockTransfer entity)
        where T : StockTransferListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Number = entity.Number,
            Reference = entity.Reference,
            State = entity.State.ToString(),
            SourceLocationId = entity.SourceLocationId,
            DestinationLocationId = entity.DestinationLocationId,
            TotalItems = entity.TransferItems.Count,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}
