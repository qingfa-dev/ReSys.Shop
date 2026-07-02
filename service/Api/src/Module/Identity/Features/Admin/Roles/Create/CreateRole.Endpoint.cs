using Shared.Security.Authorization.Attributes;

namespace Module.Identity.Features.Admin.Roles.Create;

public static partial class CreateRole
{
    /// <summary>
    /// Represents the API endpoint for creating a new role.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for creating a new role to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a POST endpoint for creating roles.
            app.MapPost(IdentityFeature.Admin.Roles.Create.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                // Create: Construct a command from the incoming request.
                var command = new Command(request);
                // Send: Dispatch the command to the mediator for processing.
                var result = await sender.Send(command, ct);
                // Map: Convert the result to an IResult for the HTTP response.
                return result.ToResult();
            })
            .WithName(nameof(CreateRole))
            .WithTags(IdentityFeature.Tags.Role)
            .HasPermission(IdentityFeature.Admin.Roles.Create.Permission)
            .WithSummary(IdentityFeature.Admin.Roles.Create.Summary)
            .WithDescription(IdentityFeature.Admin.Roles.Create.Description)
            .Produces<Result<Response>>(StatusCodes.Status201Created)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status409Conflict)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}
