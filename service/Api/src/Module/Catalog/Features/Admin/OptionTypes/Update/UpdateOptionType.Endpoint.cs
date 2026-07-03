using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.OptionTypes.Update;

public static partial class UpdateOptionType
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(CatalogFeature.Admin.OptionTypes.Update.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateOptionType))
            .WithTags(CatalogFeature.Tags.OptionType)
            .HasPermission(CatalogFeature.Admin.OptionTypes.Update.Permission)
            .WithSummary(CatalogFeature.Admin.OptionTypes.Update.Summary)
            .WithDescription(CatalogFeature.Admin.OptionTypes.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
