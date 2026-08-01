using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxons.Rules.Update;

public static partial class UpdateTaxonRule
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(CatalogFeature.Admin.TaxonRules.Update.Route, async (
                [FromRoute] Guid ruleId,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request.TaxonId, ruleId, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateTaxonRule))
            .WithTags(CatalogFeature.Tags.Taxon)
            .HasPermission(CatalogFeature.Admin.TaxonRules.Update.Permission)
            .WithSummary(CatalogFeature.Admin.TaxonRules.Update.Summary)
            .WithDescription(CatalogFeature.Admin.TaxonRules.Update.Description)
            .Produces<Result<Response>>(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}