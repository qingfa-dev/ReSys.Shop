using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxons.Get.List;

public static partial class GetTaxonList
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.Taxons.GetList.Route, async (
                Guid taxonomyId,
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(taxonomyId, parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetTaxonList))
            .WithTags(CatalogFeature.Tags.Taxon)
            .HasPermission(CatalogFeature.Admin.Taxons.GetList.Permission)
            .WithSummary(CatalogFeature.Admin.Taxons.GetList.Summary)
            .WithDescription(CatalogFeature.Admin.Taxons.GetList.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
