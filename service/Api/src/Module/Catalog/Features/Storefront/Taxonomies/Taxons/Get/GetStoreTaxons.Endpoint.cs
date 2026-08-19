using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Taxonomies.Taxons.Get;

public static partial class GetStoreTaxons
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Storefront.Taxonomies.Taxons.Get.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetStoreTaxons))
            .WithTags(CatalogFeature.Tags.Taxon)
            .WithSummary(CatalogFeature.Storefront.Taxonomies.Taxons.Get.Summary)
            .WithDescription(CatalogFeature.Storefront.Taxonomies.Taxons.Get.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}