using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockLocations.SetDefault;

public static partial class SetDefaultStockLocation
{
    /// <summary>Sets the default stock location.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: PUT /admin/inventory/stock-locations/{id}/default — sets the default stock location
            app.MapPut(InventoryFeature.Admin.StockLocations.SetDefault.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(SetDefaultStockLocation))
            .WithTags(InventoryFeature.Tags.StockLocation)
            .HasPermission(InventoryFeature.Admin.StockLocations.SetDefault.Permission)
            .WithSummary(InventoryFeature.Admin.StockLocations.SetDefault.Summary)
            .WithDescription(InventoryFeature.Admin.StockLocations.SetDefault.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict);
        }
    }
}