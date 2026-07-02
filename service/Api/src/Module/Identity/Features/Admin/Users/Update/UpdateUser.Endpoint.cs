using Shared.Security.Authorization.Attributes;

namespace Module.Identity.Features.Admin.Users.Update;

public static partial class UpdateUser
{
    /// <summary>
    /// Represents the API endpoint for updating a user.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for updating a user to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a PUT endpoint for updating users by ID.
            app.MapPut(IdentityFeature.Admin.Users.Update.Route, async (
                [FromRoute] Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                // Create: Construct a command from the route ID and request body.
                // We ensure the ID from the route is used.
                var command = new Command(id, request);
                // Send: Dispatch the command to the mediator for processing.
                var result = await sender.Send(command, ct);
                // Map: Convert the result to an IResult for the HTTP response.
                return result.ToResult();
            })
            .WithName(nameof(UpdateUser))
            .WithTags(IdentityFeature.Tags.User)
            .HasPermission(IdentityFeature.Admin.Users.Update.Permission)
            .WithSummary(IdentityFeature.Admin.Users.Update.Summary)
            .WithDescription(IdentityFeature.Admin.Users.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
