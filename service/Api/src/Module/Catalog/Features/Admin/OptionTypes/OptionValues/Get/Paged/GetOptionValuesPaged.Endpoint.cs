using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Get.Paged;

public static partial class GetOptionValuesPaged
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.OptionTypes.OptionValues.GetAll.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetOptionValuesPaged))
            .WithTags(CatalogFeature.Tags.OptionValue)
            .HasPermission(CatalogFeature.Admin.OptionTypes.OptionValues.GetAll.Permission)
            .WithSummary(CatalogFeature.Admin.OptionTypes.OptionValues.GetAll.Summary)
            .WithDescription(CatalogFeature.Admin.OptionTypes.OptionValues.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized);
        }
    }
}