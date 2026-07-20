namespace Module.Identity.Features.Admin.Users.Roles.Revoke;

public static partial class RevokeUserRoles
{
    /// <summary>Maps the user role revocation route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/admin/users/{id}/roles/revoke — revoke roles from a user
            app.MapPost(IdentityFeature.Admin.Users.Roles.Revoke.Route, async (
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
            .HasPermission(IdentityFeature.Admin.Users.Roles.Revoke.Permission)
            .WithName(nameof(RevokeUserRoles))
            .WithTags(IdentityFeature.Tags.User)
            .WithSummary(IdentityFeature.Admin.Users.Roles.Revoke.Summary)
            .WithDescription(IdentityFeature.Admin.Users.Roles.Revoke.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}