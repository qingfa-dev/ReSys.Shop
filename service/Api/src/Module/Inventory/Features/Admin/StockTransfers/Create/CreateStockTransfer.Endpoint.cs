using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockTransfers.Create;

public static partial class CreateStockTransfer
{
    /// <summary>Registers the create stock transfer endpoint.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(InventoryFeature.Admin.StockTransfers.Create.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateStockTransfer))
            .WithTags(InventoryFeature.Tags.StockTransfer)
            .HasPermission(InventoryFeature.Admin.StockTransfers.Create.Permission)
            .WithSummary(InventoryFeature.Admin.StockTransfers.Create.Summary)
            .WithDescription(InventoryFeature.Admin.StockTransfers.Create.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}