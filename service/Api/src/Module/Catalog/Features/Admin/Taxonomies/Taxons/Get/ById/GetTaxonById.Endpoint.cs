using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Get.ById;

public static partial class GetTaxonById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.Taxonomies.Taxons.GetById.Route, async (
                [FromRoute] Guid taxonomyId,
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(taxonomyId, id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetTaxonById))
            .WithTags(CatalogFeature.Tags.Taxon)
            .HasPermission(CatalogFeature.Admin.Taxonomies.Taxons.GetById.Permission)
            .WithSummary(CatalogFeature.Admin.Taxonomies.Taxons.GetById.Summary)
            .WithDescription(CatalogFeature.Admin.Taxonomies.Taxons.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}