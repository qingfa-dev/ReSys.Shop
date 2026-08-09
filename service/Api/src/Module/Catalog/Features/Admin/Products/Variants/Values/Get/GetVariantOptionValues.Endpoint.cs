using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Variants.Values.Get;

public static partial class GetVariantOptionValues
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.VariantOptionValues.Get.Route, async (
                [FromQuery] Guid variantId,
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(variantId, parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetVariantOptionValues))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.VariantOptionValues.Get.Permission)
            .WithSummary(CatalogFeature.Admin.VariantOptionValues.Get.Summary)
            .WithDescription(CatalogFeature.Admin.VariantOptionValues.Get.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}