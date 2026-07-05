using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Features.Admin.StockTransfers.Shared.Models;

namespace Module.Inventory.Features.Admin.StockTransfers.Shared.Mappings;

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
