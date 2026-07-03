using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.OptionTypes.Get.ById;

public static partial class GetOptionTypeById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.OptionTypes.GetById.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetOptionTypeById))
            .WithTags(CatalogFeature.Tags.OptionType)
            .HasPermission(CatalogFeature.Admin.OptionTypes.GetById.Permission)
            .WithSummary(CatalogFeature.Admin.OptionTypes.GetById.Summary)
            .WithDescription(CatalogFeature.Admin.OptionTypes.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
