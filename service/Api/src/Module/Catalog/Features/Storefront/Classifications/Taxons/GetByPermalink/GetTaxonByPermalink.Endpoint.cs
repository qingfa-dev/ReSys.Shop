using Module.Catalog.Features.Shared;
using Module.Catalog.Shared;

namespace Module.Catalog.Features.Storefront.Classifications.Taxons.GetByPermalink;

public static partial class GetTaxonByPermalink
{
    /// <summary>Maps GET api/storefront/taxons/{permalink} to the single-taxon query.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/taxons/{permalink} — retrieve taxon with breadcrumb and children
            app.MapGet(CatalogFeature.Storefront.Taxons.Permalink.Route, async (
                [FromRoute] string permalink,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(permalink), ct);
                return result.ToResult();
            })
            .WithName(nameof(GetTaxonByPermalink))
            .WithTags(CatalogFeature.Tags.Taxon)
            .WithSummary(CatalogFeature.Storefront.Taxons.Permalink.Summary)
            .WithDescription(CatalogFeature.Storefront.Taxons.Permalink.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}