using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.Get.ByTaxonPermalink;

public static partial class GetProductsByTaxonPermalink
{
    /// <summary>Maps GET api/storefront/taxons/{permalink}/products to the per-taxon product list.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/classifications/taxons/{permalink}/products — paged products under a taxon
            app.MapGet(CatalogFeature.Storefront.Taxons.Products.Route, async (
                [FromRoute] string permalink,
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(permalink, parameters), ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetProductsByTaxonPermalink))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Taxons.Products.Summary)
            .WithDescription(CatalogFeature.Storefront.Taxons.Products.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}