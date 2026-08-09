using Module.Catalog.Features.Shared;
using Module.Catalog.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Delete;

public static partial class DeleteEmbedding
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                CatalogFeature.Admin.VariantImageEmbeddings.Delete.Route,
                async (Guid variantImageId, ISender sender, CancellationToken ct) =>
                {
                    var result = await sender.Send(new Command(variantImageId), ct);
                    return result.ToResult();
                })
            .WithName(nameof(DeleteEmbedding))
            .WithTags(CatalogFeature.Tags.Variant)
            .HasPermission(CatalogFeature.Admin.VariantImageEmbeddings.Delete.Permission)
            .WithSummary(CatalogFeature.Admin.VariantImageEmbeddings.Delete.Summary)
            .WithDescription(CatalogFeature.Admin.VariantImageEmbeddings.Delete.Description)
            .Produces<Result<DeleteEmbedding.Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
