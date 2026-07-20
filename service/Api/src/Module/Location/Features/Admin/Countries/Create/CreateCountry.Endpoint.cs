using Module.Location.Features.Shared;

namespace Module.Location.Features.Admin.Countries.Create;

public static partial class CreateCountry
{
    /// <summary>Admin: create a country.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST {route} → create country
            app.MapPost(pattern: LocationFeature.Admin.Countries.Create.Route, handler: async (
                    [FromBody] Request request,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    var command = new Command(Request: request);
                    var result = await sender.Send(request: command, cancellationToken: ct);
                    return result.ToResult();
                })
                .WithName(nameof(CreateCountry))
                .WithTags(tags: LocationFeature.Admin.Countries.Tags)
                .HasPermission(permission: LocationFeature.Admin.Countries.Create.Permission)
                .WithSummary(summary: LocationFeature.Admin.Countries.Create.Summary)
                .WithDescription(description: LocationFeature.Admin.Countries.Create.Description)
                .Produces<Result<Response>>(statusCode: StatusCodes.Status201Created)
                .Produces<Result>(statusCode: StatusCodes.Status400BadRequest)
                .Produces<Result>(statusCode: StatusCodes.Status401Unauthorized)
                .Produces<Result>(statusCode: StatusCodes.Status409Conflict)
                .Produces<Result>(statusCode: StatusCodes.Status422UnprocessableEntity);
        }
    }
}