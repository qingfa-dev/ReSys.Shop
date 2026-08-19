using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Taxonomies.Get;

public static partial class GetStoreTaxonomies
{
    /// <summary>Maps the taxon listing route for the storefront.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/taxons — paged listing of all taxons for a taxonomy
            app.MapGet(CatalogFeature.Storefront.Taxonomies.Get.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetStoreTaxonomies))
            .WithTags(CatalogFeature.Tags.Taxonomy)
            .WithSummary(CatalogFeature.Storefront.Taxonomies.Get.Summary)
            .WithDescription(CatalogFeature.Storefront.Taxonomies.Get.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}