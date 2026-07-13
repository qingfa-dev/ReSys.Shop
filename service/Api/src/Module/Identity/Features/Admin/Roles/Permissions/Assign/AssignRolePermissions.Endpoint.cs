namespace Module.Identity.Features.Admin.Roles.Permissions.Assign;

public static partial class AssignRolePermissions
{
    /// <summary>
    /// Represents the API endpoint for assigning permissions to a role.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for assigning permissions to a role to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a PUT endpoint for assigning permissions to a role.
            app.MapPut(IdentityFeature.Admin.Roles.Permissions.Assign.Route, async (
                Guid id,
                Request request,
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
                .HasPermission(IdentityFeature.Admin.Roles.Permissions.Assign.Permission)
                .WithName(nameof(AssignRolePermissions))
            .WithTags(IdentityFeature.Tags.Role)
            .WithSummary(IdentityFeature.Admin.Roles.Permissions.Assign.Summary)
            .WithDescription(IdentityFeature.Admin.Roles.Permissions.Assign.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}