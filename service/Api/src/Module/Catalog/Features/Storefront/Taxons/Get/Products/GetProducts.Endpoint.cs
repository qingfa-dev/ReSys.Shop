using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Taxons.Get.Products;

public static partial class GetProducts
{
    /// <summary>Maps the taxon products route for the storefront.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/taxons/{id}/products — paged products within a taxon
            app.MapGet(CatalogFeature.Storefront.Taxons.Get.Products.Route, async (
                [FromRoute] Guid id,
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id, parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetProducts))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Taxons.Get.Products.Summary)
            .WithDescription(CatalogFeature.Storefront.Taxons.Get.Products.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}