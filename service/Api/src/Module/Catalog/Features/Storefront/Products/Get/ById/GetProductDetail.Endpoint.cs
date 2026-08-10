using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.Get.Detail;

public static partial class GetProductDetail
{
    /// <summary>Maps the product detail route for the storefront.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/products/{id} — full product detail with variants and prices
            app.MapGet(CatalogFeature.Storefront.Products.Detail.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetProductDetail))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Products.Detail.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Detail.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}