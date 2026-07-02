using Shared.Security.Authorization.Attributes;

namespace Module.Identity.Features.Admin.Users.Permissions.Revoke;

public static partial class RevokeUserPermissions
{
    /// <summary>
    /// Represents the API endpoint for revoking permissions from a user.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for revoking user permissions to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a DELETE endpoint for revoking permissions by user ID.
            app.MapDelete(IdentityFeature.Admin.Users.Permissions.Revoke.Route, async (
                Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                // Create: Construct a command from the route ID and request body.
                var command = new Command(id, request);
                // Send: Dispatch the command to the mediator for processing.
                var result = await sender.Send(command, ct);
                // Map: Convert the result to an IResult for the HTTP response.
                return result.ToResult();
            })
            .RequireAuthorization()
            .HasPermission(IdentityFeature.Admin.Users.Permissions.Revoke.Permission)
            .WithName(nameof(RevokeUserPermissions))
            .WithTags(IdentityFeature.Tags.User)
            .WithSummary(IdentityFeature.Admin.Users.Permissions.Revoke.Summary)
            .WithDescription(IdentityFeature.Admin.Users.Permissions.Revoke.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
