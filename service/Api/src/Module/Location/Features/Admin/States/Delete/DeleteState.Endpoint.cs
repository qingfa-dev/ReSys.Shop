using Module.Location.Features.Shared;

namespace Module.Location.Features.Admin.States.Delete;

public static partial class DeleteState
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(pattern: LocationFeature.Admin.States.Delete.Route, handler: async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(Id: id);
                var result = await sender.Send(request: command, cancellationToken: ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteState))
            .WithTags(tags: LocationFeature.Admin.States.Tags)
            .WithSummary(summary: LocationFeature.Admin.States.Delete.Summary)
            .WithDescription(description: LocationFeature.Admin.States.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<Result>(statusCode: StatusCodes.Status401Unauthorized)
            .Produces<Result>(statusCode: StatusCodes.Status404NotFound);
        }
    }
}