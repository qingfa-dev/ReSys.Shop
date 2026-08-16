using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.OptionTypes.Get.PagedOrAll;

public static partial class GetOptionTypesPagedOrAll
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.OptionTypes.GetAll.Route, async (
                [AsParameters] QueryingParameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetOptionTypesPagedOrAll))
            .WithTags(CatalogFeature.Tags.OptionType)
            .HasPermission(CatalogFeature.Admin.OptionTypes.GetAll.Permission)
            .WithSummary(CatalogFeature.Admin.OptionTypes.GetAll.Summary)
            .WithDescription(CatalogFeature.Admin.OptionTypes.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized);
        }
    }
}