using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Optiontypes.Values.Get.PagedOrAll;

public static partial class GetOptionValuePagedOrAll
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.OptionValues.GetAll.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetOptionValuePagedOrAll))
            .WithTags(CatalogFeature.Tags.OptionType)
            .HasPermission(CatalogFeature.Admin.OptionValues.GetAll.Permission)
            .WithSummary(CatalogFeature.Admin.OptionValues.GetAll.Summary)
            .WithDescription(CatalogFeature.Admin.OptionValues.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized);
        }
    }
}