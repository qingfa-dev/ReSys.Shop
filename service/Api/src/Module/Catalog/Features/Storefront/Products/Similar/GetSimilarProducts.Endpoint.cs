using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.Get.Similar;

public static partial class GetSimilarProducts
{
    /// <summary>Maps the similar products route for the storefront.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/products/similar?productId= — embedding-based similarity search
            app.MapGet(CatalogFeature.Storefront.Products.Similar.Route, async (
                [FromQuery] Guid productId,
                ISender sender,
                CancellationToken ct,
                [FromQuery] int topK = 20) =>
            {
                var query = new Query(productId, topK);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetSimilarProducts))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Products.Similar.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Similar.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}