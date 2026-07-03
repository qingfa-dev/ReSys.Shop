namespace Module.Identity.Features.Admin.Users.Roles.Assign;

public static partial class AssignUserRoles
{
    /// <summary>
    /// Represents the API endpoint for assigning roles to a user.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(IdentityFeature.Admin.Users.Roles.Assign.Route, async (
                Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .HasPermission(IdentityFeature.Admin.Users.Roles.Assign.Permission)
            .WithName(nameof(AssignUserRoles))
            .WithTags(IdentityFeature.Tags.User)
            .WithSummary(IdentityFeature.Admin.Users.Roles.Assign.Summary)
            .WithDescription(IdentityFeature.Admin.Users.Roles.Assign.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
