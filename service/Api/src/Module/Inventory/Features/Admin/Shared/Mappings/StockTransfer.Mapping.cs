using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Features.Admin.Shared.Models;

namespace Module.Inventory.Features.Admin.Shared.Mappings;

public static partial class StockTransferMapping
{
    public static Result<StockTransfer> MapToDomain<T>(this T request)
        where T : StockTransferRequest
    {
        var items = request.Items
            .Select(i => (i.VariantId, i.Quantity))
            .ToList();

        return StockTransferExtensions.Create(
            reference: request.Reference,
            sourceLocationId: request.SourceLocationId,
            destinationLocationId: request.DestinationLocationId,
            items: items);
    }
}

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
