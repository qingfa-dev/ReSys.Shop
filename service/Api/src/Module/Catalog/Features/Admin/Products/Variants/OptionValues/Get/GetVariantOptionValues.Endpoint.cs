using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.OptionValues.Get;

public static partial class GetVariantOptionValues
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.Products.Variants.OptionValues.Get.Route, async (
                Guid variantId,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(variantId);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetVariantOptionValues))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.Products.Variants.OptionValues.Get.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Variants.OptionValues.Get.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Variants.OptionValues.Get.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
