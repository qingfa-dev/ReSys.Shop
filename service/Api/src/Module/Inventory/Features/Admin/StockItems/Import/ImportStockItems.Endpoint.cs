using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockItems.Import;

public static partial class ImportStockItems
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(InventoryFeature.Admin.StockItems.Import.Route, async (
                IFormFile file,
                ISender sender,
                CancellationToken ct) =>
            {
                var request = new Request { File = file };
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(ImportStockItems))
            .WithTags(InventoryFeature.Tags.StockItem)
            .HasPermission(InventoryFeature.Admin.StockItems.Import.Permission)
            .WithSummary(InventoryFeature.Admin.StockItems.Import.Summary)
            .WithDescription(InventoryFeature.Admin.StockItems.Import.Description)
            .DisableAntiforgery()
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}