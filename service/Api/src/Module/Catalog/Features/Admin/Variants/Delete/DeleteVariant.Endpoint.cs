using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Variants.Delete;

public static partial class DeleteVariant
{
    /// <summary>
    /// DELETE endpoint that soft-deletes a variant by ID.
    /// Route: api/admin/catalog/variants/{id:guid}
    /// Permission: Products.Variants.Delete
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(CatalogFeature.Admin.Variants.Delete.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteVariant))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.Variants.Delete.Permission)
            .WithSummary(CatalogFeature.Admin.Variants.Delete.Summary)
            .WithDescription(CatalogFeature.Admin.Variants.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict);
        }
    }
}