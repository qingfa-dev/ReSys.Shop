using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Variants.Images.Download;

public static partial class DownloadVariantImage
{
    /// <summary>
    /// GET endpoint that streams the binary content of a variant image file.
    /// Route: api/admin/catalog/variant-images/{id:guid}/download
    /// Permission: Products.VariantImageMethod.View
    /// Returns: Binary file stream (not JSON)
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.VariantImages.Download.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                // Dispatch: Download query via MediatR pipeline
                var query = new Query(id);
                var result = await sender.Send(query, ct);

                // Check: Return structured error response on failure
                if (result.IsFailure)
                    return result.ToResult();

                // Serve: Stream the binary file with content type and filename
                var response = result.Value;
                return Results.File(response.Stream, response.ContentType, response.FileName);
            })
            .WithName(nameof(DownloadVariantImage))
            .WithTags(CatalogFeature.Tags.Variant)
            .WithSummary(CatalogFeature.Admin.VariantImages.Download.Summary)
            .WithDescription(CatalogFeature.Admin.VariantImages.Download.Description)
            .Produces(StatusCodes.Status200OK, contentType: "application/octet-stream")
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}