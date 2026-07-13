namespace Module.Identity.Features.Admin.Users.Delete;

public static partial class DeleteUser
{
    /// <summary>
    /// Represents the API endpoint for deleting a user.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for deleting a user to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a DELETE endpoint for users by ID.
            app.MapDelete(IdentityFeature.Admin.Users.Delete.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                // Create: Construct a command with the user ID from the route.
                var command = new Command(new Request { Id = id });
                // Send: Dispatch the command to the mediator for processing.
                var result = await sender.Send(command, ct);
                // Map: Convert the result to an IResult for the HTTP response.
                return result.ToResult();
            })
            .WithName(nameof(DeleteUser))
            .WithTags(IdentityFeature.Tags.User)
            .HasPermission(IdentityFeature.Admin.Users.Delete.Permission)
            .WithSummary(IdentityFeature.Admin.Users.Delete.Summary)
            .WithDescription(IdentityFeature.Admin.Users.Delete.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}