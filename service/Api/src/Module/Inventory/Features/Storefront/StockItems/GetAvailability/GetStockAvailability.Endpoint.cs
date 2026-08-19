using Module.Inventory.Features.Shared;
using Module.Inventory.Services;

namespace Module.Inventory.Features.Storefront.StockItems.GetAvailability;

public static partial class GetStockAvailability
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryFeature.Storefront.StockItems.GetAvailability.Route, async (
                [FromRoute] Guid variantId,
                IStockItemService stockItemService,
                HttpContext httpContext,
                CancellationToken ct) =>
            {
                var cartToken = httpContext.Items["CartToken"]?.ToString();
                var result = await stockItemService.GetAvailabilityForCartAsync(
                    variantId, cartToken, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetStockAvailability))
            .WithTags(InventoryFeature.Tags.StockItem)
            .WithSummary(InventoryFeature.Storefront.StockItems.GetAvailability.Summary)
            .WithDescription(InventoryFeature.Storefront.StockItems.GetAvailability.Description)
            .Produces<Result<VariantStockAvailability>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }

    public sealed class Validator : AbstractValidator<Guid>
    {
        public Validator() { RuleFor(x => x).NotEmpty(); }
    }
}
