using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Storefront.StockAvailability.Check;

public static partial class GetStockAvailability
{
    /// <summary>Checks stock availability for a variant.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /inventory/stock/availability/{variantId} — checks stock availability for a variant
            app.MapGet(InventoryFeature.Storefront.StockItems.Check.Route, async (
                [FromRoute] Guid variantId,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(new Request { VariantId = variantId });
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetStockAvailability))
            .WithTags(InventoryFeature.Tags.StockItem)
            .WithSummary(InventoryFeature.Storefront.StockItems.Check.Summary)
            .WithDescription(InventoryFeature.Storefront.StockItems.Check.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}