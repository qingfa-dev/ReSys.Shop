using Module.Catalog.Features.Shared;
using Module.Catalog.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Upload;

public static partial class UploadVariantImage
{
    /// <summary>
    /// POST endpoint that uploads a new image for a variant.
    /// Route: api/admin/catalog/variant-images
    /// Permission: Products.VariantImageMethod.Upload
    /// Accepts: multipart/form-data
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.VariantImages.Upload.Route, async (
                [FromForm] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                // Dispatch: Upload command via MediatR pipeline
                var command = new Command(request.VariantId, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UploadVariantImage))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.VariantImages.Upload.Permission)
            .WithSummary(CatalogFeature.Admin.VariantImages.Upload.Summary)
            .WithDescription(CatalogFeature.Admin.VariantImages.Upload.Description)
            .DisableAntiforgery()
            .Accepts<Request>("multipart/form-data")
            .Produces<Result<Response>>(StatusCodes.Status201Created)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}