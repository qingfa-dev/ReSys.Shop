using Module.Catalog.Features.Shared;
using Module.Catalog.Shared;

namespace Module.Catalog.Features.Admin.Taxonomies.Get.PagedOrAll;

public static partial class GetTaxonomiesPagedOrAll
{
    /// <summary>Maps the paged taxonomy listing route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/admin/taxonomies — paged taxonomy listing with filtering and sorting
            app.MapGet(CatalogFeature.Admin.Taxonomies.GetAll.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetTaxonomiesPagedOrAll))
            .WithTags(CatalogFeature.Tags.Taxonomy)
            .HasPermission(CatalogFeature.Admin.Taxonomies.GetAll.Permission)
            .WithSummary(CatalogFeature.Admin.Taxonomies.GetAll.Summary)
            .WithDescription(CatalogFeature.Admin.Taxonomies.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}