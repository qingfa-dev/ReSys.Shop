using Module.Location.Features.Shared;

namespace Module.Location.Features.Admin.Countries.Update;

public static partial class UpdateCountry
{
    // ============ ENDPOINT ============
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(pattern: LocationFeature.Admin.Countries.Update.Route, handler: async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(Id: id, Request: request);
                var result = await sender.Send(request: command, cancellationToken: ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateCountry))
            .WithTags(tags: LocationFeature.Admin.Countries.Tags)
            .WithSummary(summary: LocationFeature.Admin.Countries.Update.Summary)
            .WithDescription(description: LocationFeature.Admin.Countries.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<Result>(statusCode: StatusCodes.Status401Unauthorized)
            .Produces<Result>(statusCode: StatusCodes.Status404NotFound)
            .Produces<Result>(statusCode: StatusCodes.Status422UnprocessableEntity);
        }
    }
}