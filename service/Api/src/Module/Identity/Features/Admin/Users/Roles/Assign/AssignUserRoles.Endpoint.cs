namespace Module.Identity.Features.Shared.Admin.Users.Roles.Assign;

public static partial class AssignUserRoles
{
    /// <summary>Maps the user role assignment route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/admin/users/{id}/roles/assign — assign roles to a user
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