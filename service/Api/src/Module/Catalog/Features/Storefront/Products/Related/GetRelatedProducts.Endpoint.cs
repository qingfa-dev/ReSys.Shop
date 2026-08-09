using Module.Catalog.Features.Shared;
using Module.Catalog.Shared;

namespace Module.Catalog.Features.Storefront.Products.Get.Related;

public static partial class GetRelatedProducts
{
    /// <summary>Maps the related products route for the storefront.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/products/related?productId= — taxon-based related product listing
            app.MapGet(CatalogFeature.Storefront.Products.Related.Route, async (
                [FromQuery] Guid productId,
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(productId, parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetRelatedProducts))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Products.Related.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Related.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}