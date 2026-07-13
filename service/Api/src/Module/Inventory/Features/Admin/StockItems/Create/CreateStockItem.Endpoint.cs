using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockItems.Create;

public static partial class CreateStockItem
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(InventoryFeature.Admin.StockItems.Create.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateStockItem))
            .WithTags(InventoryFeature.Tags.StockItem)
            .HasPermission(InventoryFeature.Admin.StockItems.Create.Permission)
            .WithSummary(InventoryFeature.Admin.StockItems.Create.Summary)
            .WithDescription(InventoryFeature.Admin.StockItems.Create.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}