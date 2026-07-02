using Shared.Security.Authorization.Attributes;

namespace Module.Identity.Features.Admin.Roles.Update;

public static partial class UpdateRole
{
    /// <summary>
    /// Represents the API endpoint for updating a role.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for updating a role to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a PUT endpoint for updating roles by ID.
            app.MapPut(IdentityFeature.Admin.Roles.Update.Route, async (
                Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                // Create: Construct a command from the route ID and request body.
                var command = new Command(Id: id, Request: request);
                // Send: Dispatch the command to the mediator for processing.
                var result = await sender.Send(command, ct);
                // Map: Convert the result to an IResult for the HTTP response.
                return result.ToResult();
            })
            .WithName(nameof(UpdateRole))
            .WithTags(IdentityFeature.Tags.Role)
            .HasPermission(IdentityFeature.Admin.Roles.Update.Permission)
            .WithSummary(IdentityFeature.Admin.Roles.Update.Summary)
            .WithDescription(IdentityFeature.Admin.Roles.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status403Forbidden)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
