using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxons.Rules.Get;

public static partial class GetTaxonRules
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.TaxonRules.GetAll.Route, async (
                [FromQuery] Guid taxonId,
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(taxonId, parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetTaxonRules))
            .WithTags(CatalogFeature.Tags.Taxon)
            .HasPermission(CatalogFeature.Admin.TaxonRules.GetAll.Permission)
            .WithSummary(CatalogFeature.Admin.TaxonRules.GetAll.Summary)
            .WithDescription(CatalogFeature.Admin.TaxonRules.GetAll.Description)
            .Produces<PagedResult<Response>>(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
