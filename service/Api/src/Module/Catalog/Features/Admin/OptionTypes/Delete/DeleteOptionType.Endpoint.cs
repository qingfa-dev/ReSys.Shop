using Module.Catalog.Features.Shared;
using Module.Catalog.Shared;

namespace Module.Catalog.Features.Admin.OptionTypes.Delete;

public static partial class DeleteOptionType
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(CatalogFeature.Admin.OptionTypes.Delete.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteOptionType))
            .WithTags(CatalogFeature.Tags.OptionType)
            .HasPermission(CatalogFeature.Admin.OptionTypes.Delete.Permission)
            .WithSummary(CatalogFeature.Admin.OptionTypes.Delete.Summary)
            .WithDescription(CatalogFeature.Admin.OptionTypes.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict);
        }
    }
}