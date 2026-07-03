using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxonomies.Restore;

public static partial class RestoreTaxonomy
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch(CatalogFeature.Admin.Taxonomies.Restore.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(RestoreTaxonomy))
            .WithTags(CatalogFeature.Tags.Taxonomy)
            .HasPermission(CatalogFeature.Admin.Taxonomies.Restore.Permission)
            .WithSummary(CatalogFeature.Admin.Taxonomies.Restore.Summary)
            .WithDescription(CatalogFeature.Admin.Taxonomies.Restore.Description)
            .Produces<Result>(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict);
        }
    }
}
