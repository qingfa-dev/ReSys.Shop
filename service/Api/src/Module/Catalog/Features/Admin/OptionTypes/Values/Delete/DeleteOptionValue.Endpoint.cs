using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.Optiontypes.Values.Delete;

public static partial class DeleteOptionValue
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(CatalogFeature.Admin.OptionValues.Delete.Route, async (
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
            .HasPermission(CatalogFeature.Admin.OptionValues.Delete.Permission)
            .WithSummary(CatalogFeature.Admin.OptionValues.Delete.Summary)
            .WithDescription(CatalogFeature.Admin.OptionValues.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}