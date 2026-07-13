using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Delete;

public static partial class DeleteTaxon
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(CatalogFeature.Admin.Taxonomies.Taxons.Delete.Route, async (
                [FromRoute] Guid taxonomyId,
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(taxonomyId, id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteTaxon))
            .WithTags(CatalogFeature.Tags.Taxon)
            .HasPermission(CatalogFeature.Admin.Taxonomies.Taxons.Delete.Permission)
            .WithSummary(CatalogFeature.Admin.Taxonomies.Taxons.Delete.Summary)
            .WithDescription(CatalogFeature.Admin.Taxonomies.Taxons.Delete.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict);
        }
    }
}