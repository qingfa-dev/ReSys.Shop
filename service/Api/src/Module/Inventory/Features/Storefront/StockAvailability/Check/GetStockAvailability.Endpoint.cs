using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Storefront.StockAvailability.Check;

public static partial class GetStockAvailability
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryFeature.Storefront.StockAvailability.Check.Route, async (
                [FromRoute] Guid variantId,
                [FromQuery] string? cartToken,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(variantId, cartToken);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetStockAvailability))
            .WithTags(InventoryFeature.Tags.StockItem)
            .WithSummary(InventoryFeature.Storefront.StockAvailability.Check.Summary)
            .WithDescription(InventoryFeature.Storefront.StockAvailability.Check.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
