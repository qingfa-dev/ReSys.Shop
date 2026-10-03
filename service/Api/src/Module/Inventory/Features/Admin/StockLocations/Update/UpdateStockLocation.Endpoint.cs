using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockLocations.Update;

public static partial class UpdateStockLocation
{
    /// <summary>Updates a stock location.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: PUT /admin/inventory/stock-locations/{id} — updates a stock location
            app.MapPut(InventoryFeature.Admin.StockLocations.Update.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateStockLocation))
            .WithTags(InventoryFeature.Tags.StockLocation)
            .HasPermission(InventoryFeature.Admin.StockLocations.Update.Permission)
            .WithSummary(InventoryFeature.Admin.StockLocations.Update.Summary)
            .WithDescription(InventoryFeature.Admin.StockLocations.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}