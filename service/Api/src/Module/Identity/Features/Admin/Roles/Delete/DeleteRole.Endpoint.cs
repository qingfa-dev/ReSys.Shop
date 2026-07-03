namespace Module.Identity.Features.Admin.Roles.Delete;

public static partial class DeleteRole
{
    /// <summary>
    /// Represents the API endpoint for deleting a role.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for deleting a role to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a DELETE endpoint for roles by ID.
            app.MapDelete(IdentityFeature.Admin.Roles.Delete.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                // Create: Construct a command with the role ID from the route.
                var command = new Command(new Request { Id = id });
                // Send: Dispatch the command to the mediator for processing.
                var result = await sender.Send(command, ct);
                // Map: Convert the result to an IResult for the HTTP response.
                return result.ToResult();
            })
            .WithName(nameof(DeleteRole))
            .WithTags(IdentityFeature.Tags.Role)
            .HasPermission(IdentityFeature.Admin.Roles.Delete.Permission)
            .WithSummary(IdentityFeature.Admin.Roles.Delete.Summary)
            .WithDescription(IdentityFeature.Admin.Roles.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status403Forbidden)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
