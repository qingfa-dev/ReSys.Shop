using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.Get.Similar;

public static partial class GetSimilarProducts
{
    /// <summary>Maps the similar products route for the storefront.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/products/{id}/similar — embedding-based similarity search
            app.MapGet(CatalogFeature.Storefront.Products.Get.Similar.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct,
                [FromQuery] int topK = 20) =>
            {
                var query = new Query(id, topK);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetSimilarProducts))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Products.Get.Similar.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Get.Similar.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}