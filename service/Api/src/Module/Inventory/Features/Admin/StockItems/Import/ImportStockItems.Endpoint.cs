using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockItems.Import;

public static partial class ImportStockItems
{
    /// <summary>Registers the endpoint.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(InventoryFeature.Admin.StockItems.Import.Route, async (
                IFormFile file,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(file);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(ImportStockItems))
            .WithTags(InventoryFeature.Tags.StockItem)
            .HasPermission(InventoryFeature.Admin.StockItems.Import.Permission)
            .WithSummary(InventoryFeature.Admin.StockItems.Import.Summary)
            .WithDescription(InventoryFeature.Admin.StockItems.Import.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .DisableAntiforgery();
        }
    }
}
