using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.Get.Availability;

public static partial class GetAvailability
{
    /// <summary>Maps the variant availability check route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/variants/{id}/availability — stock availability across locations
            app.MapGet(CatalogFeature.Storefront.Products.Get.Availability.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetAvailability))
            .WithTags(CatalogFeature.Tags.Variant)
            .WithSummary(CatalogFeature.Storefront.Products.Get.Availability.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Get.Availability.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}