using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Models;
using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Regenerate;

public static partial class RegenerateEmbedding
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(CatalogFeature.Admin.Products.Variants.Images.Embeddings.Regenerate.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request? request,
                ISender sender,
                CancellationToken ct) =>
            {
                var modelName = string.IsNullOrEmpty(request?.ModelName)
                    ? VariantImageConstant.Defaults.DefaultEmbeddingModel
                    : request.ModelName;
                var modelVersion = request?.ModelVersion ?? string.Empty;

                var command = new Command(id, modelName, modelVersion);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(RegenerateEmbedding))
            .WithTags(CatalogFeature.Tags.VariantImage)
            .HasPermission(CatalogFeature.Admin.Products.Variants.Images.Embeddings.Regenerate.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Variants.Images.Embeddings.Regenerate.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Variants.Images.Embeddings.Regenerate.Description)
            .Produces<Result<EmbeddingDetailResponse>>(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
