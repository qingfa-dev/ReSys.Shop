using Module.Catalog.Features.Shared;
using Module.Catalog.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Update;

public static partial class UpdateVariantImage
{
    /// <summary>
    /// PUT endpoint that updates a variant image's metadata (alt, position, type).
    /// Route: api/admin/catalog/variant-images/{id:guid}
    /// Permission: Products.VariantImageMethod.Update
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(CatalogFeature.Admin.VariantImages.Update.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                // Dispatch: Update command via MediatR pipeline
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateVariantImage))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.VariantImages.Update.Permission)
            .WithSummary(CatalogFeature.Admin.VariantImages.Update.Summary)
            .WithDescription(CatalogFeature.Admin.VariantImages.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}