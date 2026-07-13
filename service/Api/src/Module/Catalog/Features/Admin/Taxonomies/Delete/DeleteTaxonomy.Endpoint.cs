using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxonomies.Delete;

public static partial class DeleteTaxonomy
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(CatalogFeature.Admin.Taxonomies.Delete.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteTaxonomy))
            .WithTags(CatalogFeature.Tags.Taxonomy)
            .HasPermission(CatalogFeature.Admin.Taxonomies.Delete.Permission)
            .WithSummary(CatalogFeature.Admin.Taxonomies.Delete.Summary)
            .WithDescription(CatalogFeature.Admin.Taxonomies.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict);
        }
    }
}