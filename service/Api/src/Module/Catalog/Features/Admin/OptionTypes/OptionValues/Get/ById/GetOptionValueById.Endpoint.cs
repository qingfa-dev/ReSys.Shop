using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Get.ById;

public static partial class GetOptionValueById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(CatalogFeature.Admin.OptionTypes.OptionValues.GetById.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetOptionValueById))
            .WithTags(CatalogFeature.Tags.OptionValue)
            .HasPermission(CatalogFeature.Admin.OptionTypes.OptionValues.GetById.Permission)
            .WithSummary(CatalogFeature.Admin.OptionTypes.OptionValues.GetById.Summary)
            .WithDescription(CatalogFeature.Admin.OptionTypes.OptionValues.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}