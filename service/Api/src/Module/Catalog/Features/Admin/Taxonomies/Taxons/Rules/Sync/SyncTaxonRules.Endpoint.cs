using Module.Catalog.Features.Shared;
using Module.Catalog.Shared;

namespace Module.Catalog.Features.Admin.Taxons.Rules.Sync;

public static partial class SyncTaxonRules
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.TaxonRules.Sync.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request.TaxonId, request);
                var result = await sender.Send(command, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(SyncTaxonRules))
            .WithTags(CatalogFeature.Tags.Taxon)
            .HasPermission(CatalogFeature.Admin.TaxonRules.Sync.Permission)
            .WithSummary(CatalogFeature.Admin.TaxonRules.Sync.Summary)
            .WithDescription(CatalogFeature.Admin.TaxonRules.Sync.Description)
            .Produces<PagedResult<Response>>(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}