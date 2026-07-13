using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxonomies.Update;

public static partial class UpdateTaxonomy
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(CatalogFeature.Admin.Taxonomies.Update.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateTaxonomy))
            .WithTags(CatalogFeature.Tags.Taxonomy)
            .HasPermission(CatalogFeature.Admin.Taxonomies.Update.Permission)
            .WithSummary(CatalogFeature.Admin.Taxonomies.Update.Summary)
            .WithDescription(CatalogFeature.Admin.Taxonomies.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}