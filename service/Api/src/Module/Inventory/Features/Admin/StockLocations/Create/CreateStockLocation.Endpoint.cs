using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockLocations.Create;

public static partial class CreateStockLocation
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(InventoryFeature.Admin.StockLocations.Create.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateStockLocation))
            .WithTags(InventoryFeature.Tags.StockLocation)
            .HasPermission(InventoryFeature.Admin.StockLocations.Create.Permission)
            .WithSummary(InventoryFeature.Admin.StockLocations.Create.Summary)
            .WithDescription(InventoryFeature.Admin.StockLocations.Create.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}