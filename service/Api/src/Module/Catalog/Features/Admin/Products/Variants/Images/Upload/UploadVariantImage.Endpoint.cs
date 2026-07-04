using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Upload;

public static partial class UploadVariantImage
{
    /// <summary>
    /// POST endpoint that uploads a new image for a variant.
    /// Route: api/catalog/products/variants/{variantId:guid}/images
    /// Permission: Products.VariantImageMethod.Upload
    /// Accepts: multipart/form-data
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.Products.Variants.Images.Upload.Route, async (
                [FromRoute] Guid variantId,
                [FromForm] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                // Dispatch: Upload command via MediatR pipeline
                var command = new Command(variantId, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UploadVariantImage))
            .WithTags(CatalogFeature.Tags.VariantImage)
            .HasPermission(CatalogFeature.Admin.Products.Variants.Images.Upload.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Variants.Images.Upload.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Variants.Images.Upload.Description)
            .DisableAntiforgery()
            .Accepts<Request>("multipart/form-data")
            .Produces<Result<Response>>(StatusCodes.Status201Created)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
