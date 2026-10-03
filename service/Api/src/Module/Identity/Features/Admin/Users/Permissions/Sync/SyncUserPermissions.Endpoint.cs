namespace Module.Identity.Features.Shared.Admin.Users.Permissions.Sync;

public static partial class SyncUserPermissions
{
    /// <summary>Maps the user permission synchronisation route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: PUT /api/admin/users/{id}/permissions/sync — sync all permission assignments for a user
            app.MapPut(IdentityFeature.Admin.Users.Permissions.Sync.Route, async (
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
            .HasPermission(IdentityFeature.Admin.Users.Permissions.Sync.Permission)
            .WithName(nameof(SyncUserPermissions))
            .WithTags(IdentityFeature.Tags.User)
            .WithSummary(IdentityFeature.Admin.Users.Permissions.Sync.Summary)
            .WithDescription(IdentityFeature.Admin.Users.Permissions.Sync.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}