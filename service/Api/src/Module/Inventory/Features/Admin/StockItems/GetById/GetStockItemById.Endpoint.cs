using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockItems.GetById;

public static partial class GetStockItemById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryFeature.Admin.StockItems.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetStockItemById))
            .WithTags(InventoryFeature.Tags.StockItem)
            .HasPermission(InventoryFeature.Admin.StockItems.GetById.Permission)
            .WithSummary(InventoryFeature.Admin.StockItems.GetById.Summary)
            .WithDescription(InventoryFeature.Admin.StockItems.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
