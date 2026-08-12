using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.Products.Images.Inferences.Get;

public static partial class GetVisualSearchModels
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Storefront.Products.Images.Inferences.Route, async (
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query();
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetVisualSearchModels))
            .WithTags(CatalogFeature.Tags.Product)
            .WithSummary(CatalogFeature.Storefront.Products.Images.Inferences.Summary)
            .WithDescription(CatalogFeature.Storefront.Products.Images.Inferences.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status502BadGateway);
        }
    }
}
