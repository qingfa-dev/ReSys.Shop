using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Classifications.Taxons;

public static partial class GetStoreTaxons
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.Taxons.GetAll.Route, async (
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
            .HasPermission(CatalogFeature.Admin.Taxons.GetAll.Permission)
            .WithSummary(CatalogFeature.Admin.Taxons.GetAll.Summary)
            .WithDescription(CatalogFeature.Admin.Taxons.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}