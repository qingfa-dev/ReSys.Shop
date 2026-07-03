using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.OptionTypes.Get.All;

public static partial class GetAllOptionTypes
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Storefront.OptionTypes.Get.All.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetAllOptionTypes))
            .WithTags(CatalogFeature.Tags.OptionType)
            .WithSummary(CatalogFeature.Storefront.OptionTypes.Get.All.Summary)
            .WithDescription(CatalogFeature.Storefront.OptionTypes.Get.All.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
