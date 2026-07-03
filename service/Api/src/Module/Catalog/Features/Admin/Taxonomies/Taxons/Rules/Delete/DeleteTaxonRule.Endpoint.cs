using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Delete;

public static partial class DeleteTaxonRule
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(CatalogFeature.Admin.Taxonomies.Taxons.Rules.Delete.Route, async (
                Guid taxonomyId,
                Guid id,
                Guid ruleId,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(taxonomyId, id, ruleId);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteTaxonRule))
            .WithTags(CatalogFeature.Tags.Taxon)
            .HasPermission(CatalogFeature.Admin.Taxonomies.Taxons.Rules.Delete.Permission)
            .WithSummary(CatalogFeature.Admin.Taxonomies.Taxons.Rules.Delete.Summary)
            .WithDescription(CatalogFeature.Admin.Taxonomies.Taxons.Rules.Delete.Description)
            .Produces<Result<Response>>(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status403Forbidden)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
