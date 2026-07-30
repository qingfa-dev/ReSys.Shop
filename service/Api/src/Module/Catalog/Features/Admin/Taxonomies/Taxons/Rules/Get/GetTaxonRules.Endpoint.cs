using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Get;

public static partial class GetTaxonRules
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.Taxonomies.Taxons.Rules.GetAll.Route, async (
                Guid id,
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id, parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetTaxonRules))
            .WithTags(CatalogFeature.Tags.Taxon)
            .HasPermission(CatalogFeature.Admin.Taxonomies.Taxons.Rules.GetAll.Permission)
            .WithSummary(CatalogFeature.Admin.Taxonomies.Taxons.Rules.GetAll.Summary)
            .WithDescription(CatalogFeature.Admin.Taxonomies.Taxons.Rules.GetAll.Description)
            .Produces<PagedResult<Response>>(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
