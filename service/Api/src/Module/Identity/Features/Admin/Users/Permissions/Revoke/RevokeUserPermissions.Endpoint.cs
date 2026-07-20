namespace Module.Identity.Features.Admin.Users.Permissions.Revoke;

public static partial class RevokeUserPermissions
{
    /// <summary>Maps the user permission revocation route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: DELETE /api/admin/users/{id}/permissions/revoke — revoke permissions from a user
            app.MapDelete(IdentityFeature.Admin.Users.Permissions.Revoke.Route, async (
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
            .HasPermission(IdentityFeature.Admin.Users.Permissions.Revoke.Permission)
            .WithName(nameof(RevokeUserPermissions))
            .WithTags(IdentityFeature.Tags.User)
            .WithSummary(IdentityFeature.Admin.Users.Permissions.Revoke.Summary)
            .WithDescription(IdentityFeature.Admin.Users.Permissions.Revoke.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}