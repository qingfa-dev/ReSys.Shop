using Module.Location.Features.Shared;

namespace Module.Location.Features.Admin.States.Create;

public static partial class CreateState
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(pattern: LocationFeature.Admin.States.Create.Route, handler: async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(Request: request);
                var result = await sender.Send(request: command, cancellationToken: ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateState))
            .WithTags(tags: LocationFeature.Admin.States.Tags)
            .WithSummary(summary: LocationFeature.Admin.States.Create.Summary)
            .WithDescription(description: LocationFeature.Admin.States.Create.Description)
            .Produces<Result<Response>>(statusCode: StatusCodes.Status201Created)
            .Produces<Result>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<Result>(statusCode: StatusCodes.Status401Unauthorized)
            .Produces<Result>(statusCode: StatusCodes.Status409Conflict)
            .Produces<Result>(statusCode: StatusCodes.Status422UnprocessableEntity);
        }
    }
}