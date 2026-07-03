using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Restore;

public static partial class RestoreTaxon
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch(CatalogFeature.Admin.Taxonomies.Taxons.Restore.Route, async (
                [FromRoute] Guid taxonomyId,
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(taxonomyId, id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(RestoreTaxon))
            .WithTags(CatalogFeature.Tags.Taxon)
            .HasPermission(CatalogFeature.Admin.Taxonomies.Taxons.Restore.Permission)
            .WithSummary(CatalogFeature.Admin.Taxonomies.Taxons.Restore.Summary)
            .WithDescription(CatalogFeature.Admin.Taxonomies.Taxons.Restore.Description)
            .Produces<Result>(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict);
        }
    }
}
