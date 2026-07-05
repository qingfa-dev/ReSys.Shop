using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockLocations.Delete;

public static partial class DeleteStockLocation
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(InventoryFeature.Admin.StockLocations.Delete.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteStockLocation))
            .WithTags(InventoryFeature.Tags.StockLocation)
            .HasPermission(InventoryFeature.Admin.StockLocations.Delete.Permission)
            .WithSummary(InventoryFeature.Admin.StockLocations.Delete.Summary)
            .WithDescription(InventoryFeature.Admin.StockLocations.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict);
        }
    }
}
