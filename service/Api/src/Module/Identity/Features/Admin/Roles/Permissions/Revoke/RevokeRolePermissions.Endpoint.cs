namespace Module.Identity.Features.Admin.Roles.Permissions.Revoke;

public static partial class RevokeRolePermissions
{
    /// <summary>
    /// Represents the API endpoint for revoking permissions from a role.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for revoking permissions from a role to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a DELETE endpoint for revoking permissions from a role.
            app.MapDelete(IdentityFeature.Admin.Roles.Permissions.Revoke.Route, async (
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
            .HasPermission(IdentityFeature.Admin.Roles.Permissions.Revoke.Permission)
            .WithName(nameof(RevokeRolePermissions))
            .WithTags(IdentityFeature.Tags.Role)
            .WithSummary(IdentityFeature.Admin.Roles.Permissions.Revoke.Summary)
            .WithDescription(IdentityFeature.Admin.Roles.Permissions.Revoke.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}