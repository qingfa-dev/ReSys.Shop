using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Classifications.Taxons;

public static partial class GetStoreTaxons
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Storefront.Classifications.Taxons.Route, async (
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
            .WithSummary(CatalogFeature.Storefront.Classifications.Taxons.Summary)
            .WithDescription(CatalogFeature.Storefront.Classifications.Taxons.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}