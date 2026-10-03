using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockTransfers.Get.Paged;

public static partial class GetStockTransferPagedOrAll
{
    /// <summary>Gets paged stock transfers.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /admin/inventory/stock-transfers — gets paged stock transfers
            app.MapGet(InventoryFeature.Admin.StockTransfers.GetAll.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetStockTransferPagedOrAll))
            .WithTags(InventoryFeature.Tags.StockTransfer)
            .HasPermission(InventoryFeature.Admin.StockTransfers.GetAll.Permission)
            .WithSummary(InventoryFeature.Admin.StockTransfers.GetAll.Summary)
            .WithDescription(InventoryFeature.Admin.StockTransfers.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}