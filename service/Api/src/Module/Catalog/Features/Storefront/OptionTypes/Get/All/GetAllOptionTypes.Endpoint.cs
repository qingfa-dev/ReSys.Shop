using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Storefront.OptionTypes.Get.All;

public static partial class GetAllOptionTypes
{
    /// <summary>Maps the option types listing route for the storefront.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/storefront/option-types — paged listing of product option types (size, colour)
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