using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Classifications.Taxonomies;

public static partial class GetStoreTaxonomies
{
    /// <summary>Maps the taxon listing route for the storefront.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/taxons — paged listing of all taxons for a taxonomy
            app.MapGet(CatalogFeature.Storefront.Classifications.Taxons.All.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetStoreTaxonomies))
            .WithTags(CatalogFeature.Tags.Taxon)
            .WithSummary(CatalogFeature.Storefront.Classifications.Taxons.All.Summary)
            .WithDescription(CatalogFeature.Storefront.Classifications.Taxons.All.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}