using Module.Catalog.Features.Shared;

namespace Module.Catalog.Features.Admin.OptionTypes.OptionValues.Update;

public static partial class UpdateOptionValue
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(CatalogFeature.Admin.OptionTypes.OptionValues.Update.Route, async (
                [FromRoute] Guid optionTypeId,
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(optionTypeId, id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateOptionValue))
            .WithTags(CatalogFeature.Tags.OptionValue)
            .HasPermission(CatalogFeature.Admin.OptionTypes.OptionValues.Update.Permission)
            .WithSummary(CatalogFeature.Admin.OptionTypes.OptionValues.Update.Summary)
            .WithDescription(CatalogFeature.Admin.OptionTypes.OptionValues.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}