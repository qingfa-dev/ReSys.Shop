using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Products.Classifications.Get;

public static partial class GetProductClassifications
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.Products.Classifications.Get.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetProductClassifications))
            .WithTags(CatalogFeature.Tags.Product)
            .HasPermission(CatalogFeature.Admin.Products.Classifications.Get.Permission)
            .WithSummary(CatalogFeature.Admin.Products.Classifications.Get.Summary)
            .WithDescription(CatalogFeature.Admin.Products.Classifications.Get.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
