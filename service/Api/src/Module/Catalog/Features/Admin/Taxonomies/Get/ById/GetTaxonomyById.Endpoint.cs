using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxonomies.Get.ById;

public static partial class GetTaxonomyById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.Taxonomies.GetById.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetTaxonomyById))
            .WithTags(CatalogFeature.Tags.Taxonomy)
            .HasPermission(CatalogFeature.Admin.Taxonomies.GetById.Permission)
            .WithSummary(CatalogFeature.Admin.Taxonomies.GetById.Summary)
            .WithDescription(CatalogFeature.Admin.Taxonomies.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
