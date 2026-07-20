using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Taxons.Get.All;

public static partial class GetAllTaxons
{
    /// <summary>Maps the taxon listing route for the storefront.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/taxons — paged listing of all taxons for a taxonomy
            app.MapGet(CatalogFeature.Storefront.Taxons.Get.All.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetAllTaxons))
            .WithTags(CatalogFeature.Tags.Taxon)
            .WithSummary(CatalogFeature.Storefront.Taxons.Get.All.Summary)
            .WithDescription(CatalogFeature.Storefront.Taxons.Get.All.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}