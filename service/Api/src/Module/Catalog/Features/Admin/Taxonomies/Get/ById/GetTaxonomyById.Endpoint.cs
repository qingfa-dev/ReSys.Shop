using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxonomies.Get.ById;

public static partial class GetTaxonomyById
{
    /// <summary>Maps the taxonomy detail route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/admin/taxonomies/{id} — single taxonomy by ID
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