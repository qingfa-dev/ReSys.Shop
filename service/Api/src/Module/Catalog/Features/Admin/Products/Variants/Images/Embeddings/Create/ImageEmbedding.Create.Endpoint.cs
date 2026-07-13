using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Models;
using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Create;

public static partial class CreateEmbedding
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(CatalogFeature.Admin.Products.Variants.Images.Embeddings.Create.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request? request,
                ISender sender,
                CancellationToken ct) =>
            {
                var modelName = string.IsNullOrEmpty(request?.ModelName)
                    ? VariantImageConstant.Defaults.DefaultEmbeddingModel
                    : request.ModelName;

                var command = new Command(id, modelName);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateEmbedding))
            .WithTags(CatalogFeature.Tags.VariantImage)
            .HasPermission(CatalogFeature.Admin.Products.Variants.Images.Embeddings.Create.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Variants.Images.Embeddings.Create.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Variants.Images.Embeddings.Create.Description)
            .Produces<Result<EmbeddingDetailResponse>>(StatusCodes.Status201Created)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}