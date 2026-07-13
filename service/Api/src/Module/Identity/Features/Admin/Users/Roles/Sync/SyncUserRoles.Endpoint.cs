namespace Module.Identity.Features.Admin.Users.Roles.Sync;

public static partial class SyncUserRoles
{
    /// <summary>
    /// Represents the API endpoint for synchronizing roles for a user.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch(IdentityFeature.Admin.Users.Roles.Sync.Route, async (
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
            .HasPermission(IdentityFeature.Admin.Users.Roles.Sync.Permission)
            .WithName(nameof(SyncUserRoles))
            .WithTags(IdentityFeature.Tags.User)
            .WithSummary(IdentityFeature.Admin.Users.Roles.Sync.Summary)
            .WithDescription(IdentityFeature.Admin.Users.Roles.Sync.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}