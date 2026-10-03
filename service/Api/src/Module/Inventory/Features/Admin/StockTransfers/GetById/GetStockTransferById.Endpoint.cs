using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockTransfers.GetById;

public static partial class GetStockTransferById
{
    /// <summary>Gets a stock transfer by ID.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /admin/inventory/stock-transfers/{id} — gets a stock transfer by ID
            app.MapGet(InventoryFeature.Admin.StockTransfers.GetById.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetStockTransferById))
            .WithTags(InventoryFeature.Tags.StockTransfer)
            .HasPermission(InventoryFeature.Admin.StockTransfers.GetById.Permission)
            .WithSummary(InventoryFeature.Admin.StockTransfers.GetById.Summary)
            .WithDescription(InventoryFeature.Admin.StockTransfers.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
