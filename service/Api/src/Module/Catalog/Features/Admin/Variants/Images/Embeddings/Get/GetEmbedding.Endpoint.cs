using Module.Catalog.Features.Admin.Shared.Models;
using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Get;

public static partial class GetEmbedding
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                CatalogFeature.Admin.VariantImageEmbeddings.Get.Route,
                async (Guid variantImageId, ISender sender, CancellationToken ct) =>
                {
                    var result = await sender.Send(new Query(variantImageId), ct);
                    return result.ToResult();
                })
            .WithName(nameof(GetEmbedding))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.VariantImageEmbeddings.Get.Permission)
            .WithSummary(CatalogFeature.Admin.VariantImageEmbeddings.Get.Summary)
            .WithDescription(CatalogFeature.Admin.VariantImageEmbeddings.Get.Description)
            .Produces<Result<EmbeddingDetailResponse>>(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
