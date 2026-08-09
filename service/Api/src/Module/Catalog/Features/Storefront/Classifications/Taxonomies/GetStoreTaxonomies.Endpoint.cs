using Module.Catalog.Features.Shared;
using Module.Catalog.Shared;

namespace Module.Catalog.Features.Storefront.Classifications.Taxonomies;

public static partial class GetStoreTaxonomies
{
    /// <summary>Maps the taxon listing route for the storefront.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/taxons — paged listing of all taxons for a taxonomy
            app.MapGet(CatalogFeature.Storefront.Taxonomies.Route, async (
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
            .WithSummary(CatalogFeature.Storefront.Taxonomies.Summary)
            .WithDescription(CatalogFeature.Storefront.Taxonomies.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}