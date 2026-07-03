using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Update;

public static partial class UpdateTaxon
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(CatalogFeature.Admin.Taxonomies.Taxons.Update.Route, async (
                [FromRoute] Guid taxonomyId,
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(taxonomyId, id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateTaxon))
            .WithTags(CatalogFeature.Tags.Taxon)
            .HasPermission(CatalogFeature.Admin.Taxonomies.Taxons.Update.Permission)
            .WithSummary(CatalogFeature.Admin.Taxonomies.Taxons.Update.Summary)
            .WithDescription(CatalogFeature.Admin.Taxonomies.Taxons.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
