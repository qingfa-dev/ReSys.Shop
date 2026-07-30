using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Delete;

public static partial class DeleteOptionValue
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(CatalogFeature.Admin.OptionTypes.OptionValues.Delete.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteOptionValue))
            .WithTags(CatalogFeature.Tags.OptionValue)
            .HasPermission(CatalogFeature.Admin.OptionTypes.OptionValues.Delete.Permission)
            .WithSummary(CatalogFeature.Admin.OptionTypes.OptionValues.Delete.Summary)
            .WithDescription(CatalogFeature.Admin.OptionTypes.OptionValues.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}