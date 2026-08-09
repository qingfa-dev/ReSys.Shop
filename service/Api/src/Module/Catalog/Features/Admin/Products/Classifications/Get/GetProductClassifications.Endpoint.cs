using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.ProductClassifications.Get;

public static partial class GetProductClassifications
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.ProductClassifications.Get.Route, async (
                [FromQuery] Guid productId,
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(productId, parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetProductClassifications))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.ProductClassifications.Get.Permission)
            .WithSummary(CatalogFeature.Admin.ProductClassifications.Get.Summary)
            .WithDescription(CatalogFeature.Admin.ProductClassifications.Get.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}