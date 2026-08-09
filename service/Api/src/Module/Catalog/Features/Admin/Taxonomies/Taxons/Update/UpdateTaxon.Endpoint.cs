using Module.Catalog.Features.Shared;
using Module.Catalog.Shared;

namespace Module.Catalog.Features.Admin.Taxons.Update;

public static partial class UpdateTaxon
{
    /// <summary>Maps the taxon update route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: PUT /api/admin/taxonomies/{taxonomyId}/taxons/{id} — update a taxon within a taxonomy
            app.MapPut(CatalogFeature.Admin.Taxons.Update.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateTaxon))
            .WithTags(CatalogFeature.Tags.Taxon)
            .HasPermission(CatalogFeature.Admin.Taxons.Update.Permission)
            .WithSummary(CatalogFeature.Admin.Taxons.Update.Summary)
            .WithDescription(CatalogFeature.Admin.Taxons.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}