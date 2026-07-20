using Module.Location.Features.Shared;

namespace Module.Location.Features.Admin.States.Update;

public static partial class UpdateState
{
    /// <summary>Admin: update a state.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: PUT {route}/{id} → update state
            app.MapPut(pattern: LocationFeature.Admin.States.Update.Route, handler: async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(Id: id, Request: request);
                var result = await sender.Send(request: command, cancellationToken: ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateState))
            .WithTags(tags: LocationFeature.Admin.States.Tags)
            .WithSummary(summary: LocationFeature.Admin.States.Update.Summary)
            .WithDescription(description: LocationFeature.Admin.States.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<Result>(statusCode: StatusCodes.Status401Unauthorized)
            .Produces<Result>(statusCode: StatusCodes.Status404NotFound)
            .Produces<Result>(statusCode: StatusCodes.Status422UnprocessableEntity);
        }
    }
}