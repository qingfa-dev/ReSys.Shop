using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Sync;

public static partial class SyncTaxonRules
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.Taxonomies.Taxons.Rules.Sync.Route, async (
                Guid taxonomyId,
                Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(taxonomyId, id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(SyncTaxonRules))
            .WithTags(CatalogFeature.Tags.Taxon)
            .HasPermission(CatalogFeature.Admin.Taxonomies.Taxons.Rules.Sync.Permission)
            .WithSummary(CatalogFeature.Admin.Taxonomies.Taxons.Rules.Sync.Summary)
            .WithDescription(CatalogFeature.Admin.Taxonomies.Taxons.Rules.Sync.Description)
            .Produces<Result<Response>>(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
