using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Taxonomies.Create;

public static partial class CreateTaxonomy
{
    /// <summary>Maps the taxonomy creation route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/admin/taxonomies — create a new taxonomy
            app.MapPost(CatalogFeature.Admin.Taxonomies.Create.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateTaxonomy))
            .WithTags(CatalogFeature.Tags.Taxonomy)
            .HasPermission(CatalogFeature.Admin.Taxonomies.Create.Permission)
            .WithSummary(CatalogFeature.Admin.Taxonomies.Create.Summary)
            .WithDescription(CatalogFeature.Admin.Taxonomies.Create.Description)
            .Produces<Result<Response>>(StatusCodes.Status201Created)
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}