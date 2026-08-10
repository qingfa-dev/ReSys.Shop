using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Models;
using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Regenerate;

public static partial class RegenerateEmbedding
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(CatalogFeature.Admin.VariantImageEmbeddings.Regenerate.Route, async (
                [FromBody] Request? request,
                ISender sender,
                CancellationToken ct) =>
            {
                var modelName = string.IsNullOrEmpty(request?.ModelName)
                    ? VariantImageConstant.Defaults.DefaultEmbeddingModel
                    : request.ModelName;
                var modelVersion = request?.ModelVersion ?? string.Empty;

                var command = new Command(new Request
                {
                    VariantImageId = request?.VariantImageId ?? Guid.Empty,
                    ModelName = modelName,
                    ModelVersion = modelVersion
                });
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(RegenerateEmbedding))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.VariantImageEmbeddings.Regenerate.Permission)
            .WithSummary(CatalogFeature.Admin.VariantImageEmbeddings.Regenerate.Summary)
            .WithDescription(CatalogFeature.Admin.VariantImageEmbeddings.Regenerate.Description)
            .Produces<Result<EmbeddingDetailResponse>>(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}