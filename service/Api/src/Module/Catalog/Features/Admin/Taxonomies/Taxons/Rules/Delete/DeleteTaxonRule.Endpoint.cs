using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxons.Rules.Delete;

public static partial class DeleteTaxonRule
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(CatalogFeature.Admin.TaxonRules.Delete.Route, async (
                [FromRoute] Guid ruleId,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request.TaxonId, ruleId);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteTaxonRule))
            .WithTags(CatalogFeature.Tags.Taxon)
            .HasPermission(CatalogFeature.Admin.TaxonRules.Delete.Permission)
            .WithSummary(CatalogFeature.Admin.TaxonRules.Delete.Summary)
            .WithDescription(CatalogFeature.Admin.TaxonRules.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status403Forbidden)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}