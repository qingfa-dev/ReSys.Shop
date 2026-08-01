using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.Get.Availability;

public static partial class GetAvailability
{
    /// <summary>Maps the variant availability check route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/products/availability?productId= — stock availability across locations
            app.MapGet(CatalogFeature.Storefront.Products.Availability.Route, async (
                [FromQuery] Guid productId,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(productId);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetAvailability))
            .WithTags(CatalogFeature.Tags.Variant)
            .WithSummary(CatalogFeature.Storefront.Products.Availability.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Availability.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}