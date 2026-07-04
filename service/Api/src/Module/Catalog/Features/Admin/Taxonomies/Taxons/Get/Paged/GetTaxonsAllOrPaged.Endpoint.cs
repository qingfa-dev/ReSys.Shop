using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Get.Paged;

public static partial class GetTaxonsAllOrPaged
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.Taxonomies.Taxons.GetAll.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetTaxonsAllOrPaged))
            .WithTags(CatalogFeature.Tags.Taxon)
            .HasPermission(CatalogFeature.Admin.Taxonomies.Taxons.GetAll.Permission)
            .WithSummary(CatalogFeature.Admin.Taxonomies.Taxons.GetAll.Summary)
            .WithDescription(CatalogFeature.Admin.Taxonomies.Taxons.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
