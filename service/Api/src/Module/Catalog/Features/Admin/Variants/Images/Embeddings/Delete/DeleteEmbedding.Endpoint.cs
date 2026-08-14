using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Delete;

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
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
