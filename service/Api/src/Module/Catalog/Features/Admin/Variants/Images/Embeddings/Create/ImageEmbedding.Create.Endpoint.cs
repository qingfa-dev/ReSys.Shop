using Module.Catalog.Domain.Variants.Images;
using Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Models;
using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Create;

public static partial class CreateEmbedding
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.VariantImageEmbeddings.Create.Route, async (
                [FromBody] Request? request,
                ISender sender,
                CancellationToken ct) =>
            {
                var modelName = string.IsNullOrEmpty(request?.ModelName)
                    ? VariantImageConstant.Defaults.DefaultEmbeddingModel
                    : request.ModelName;

                var command = new Command(new Request
                {
                    VariantImageId = request?.VariantImageId ?? Guid.Empty,
                    ModelName = modelName
                });
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateEmbedding))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.VariantImageEmbeddings.Create.Permission)
            .WithSummary(CatalogFeature.Admin.VariantImageEmbeddings.Create.Summary)
            .WithDescription(CatalogFeature.Admin.VariantImageEmbeddings.Create.Description)
            .Produces<Result<EmbeddingDetailResponse>>(StatusCodes.Status201Created)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}