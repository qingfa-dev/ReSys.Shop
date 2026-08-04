using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxons.Restore;

public static partial class RestoreTaxon
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch(CatalogFeature.Admin.Taxons.Restore.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(RestoreTaxon))
            .WithTags(CatalogFeature.Tags.Taxon)
            .HasPermission(CatalogFeature.Admin.Taxons.Restore.Permission)
            .WithSummary(CatalogFeature.Admin.Taxons.Restore.Summary)
            .WithDescription(CatalogFeature.Admin.Taxons.Restore.Description)
            .Produces<Result>(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict);
        }
    }
}