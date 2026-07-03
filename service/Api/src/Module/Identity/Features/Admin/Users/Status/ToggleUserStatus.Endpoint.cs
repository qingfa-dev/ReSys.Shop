namespace Module.Identity.Features.Admin.Users.Status;

public static partial class ToggleUserStatus
{
    /// <summary>
    /// Represents the API endpoint for toggling a user's active status.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for toggling user status to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a PATCH endpoint for toggling status by ID.
            // Using PATCH for a partial update/toggle operation.
            app.MapPatch(IdentityFeature.Admin.Users.Status.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                // Create: Construct a command from the route ID.
                var command = new Command(id);
                // Send: Dispatch the command to the mediator for processing.
                var result = await sender.Send(command, ct);
                // Map: Convert the result to an IResult for the HTTP response.
                return result.ToResult();
            })
            .WithName(nameof(ToggleUserStatus))
            .WithTags(IdentityFeature.Tags.User)
            .HasPermission(IdentityFeature.Admin.Users.Status.Permission)
            .WithSummary(IdentityFeature.Admin.Users.Status.Summary)
            .WithDescription(IdentityFeature.Admin.Users.Status.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
