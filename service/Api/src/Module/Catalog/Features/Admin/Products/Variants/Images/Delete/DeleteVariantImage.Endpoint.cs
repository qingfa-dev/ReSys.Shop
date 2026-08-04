using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Delete;

public static partial class DeleteVariantImage
{
    /// <summary>
    /// DELETE endpoint that removes a variant image and its storage file.
    /// Route: api/catalog/variant-images/{id:guid}
    /// Permission: Products.VariantImageMethod.Delete
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(CatalogFeature.Admin.VariantImages.Delete.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                // Dispatch: Delete command via MediatR pipeline
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteVariantImage))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.VariantImages.Delete.Permission)
            .WithSummary(CatalogFeature.Admin.VariantImages.Delete.Summary)
            .WithDescription(CatalogFeature.Admin.VariantImages.Delete.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}