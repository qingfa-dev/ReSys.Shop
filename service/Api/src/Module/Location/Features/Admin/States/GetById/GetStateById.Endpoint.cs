using Module.Location.Features.Shared;

namespace Module.Location.Features.Admin.States.GetById;

public static partial class GetStateById
{
    /// <summary>Admin: get state by ID.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET {route}/{id} → state by ID for admin
            app.MapGet(pattern: LocationFeature.Admin.States.GetById.Route, handler: async (
                [FromRoute] Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(Id: id);
                var result = await sender.Send(request: query, cancellationToken: ct);
                return result.ToResult();
            })
            .WithName(nameof(GetStateById))
            .WithTags(tags: LocationFeature.Admin.States.Tags)
            .WithSummary(summary: LocationFeature.Admin.States.GetById.Summary)
            .WithDescription(description: LocationFeature.Admin.States.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(statusCode: StatusCodes.Status400BadRequest)
            .Produces<Result>(statusCode: StatusCodes.Status401Unauthorized)
            .Produces<Result>(statusCode: StatusCodes.Status404NotFound);
        }
    }
}