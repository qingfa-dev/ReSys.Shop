namespace Module.Identity.Features.Admin.Users.Permissions.Get;

public static partial class GetUserPermissions
{
    /// <summary>Maps the user permission retrieval route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/admin/users/{id}/permissions — get permissions for a user
            app.MapGet(IdentityFeature.Admin.Users.Permissions.Get.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .HasPermission(IdentityFeature.Admin.Users.Permissions.Get.Permission)
            .WithName(nameof(GetUserPermissions))
            .WithTags(IdentityFeature.Tags.User)
            .WithSummary(IdentityFeature.Admin.Users.Permissions.Get.Summary)
            .WithDescription(IdentityFeature.Admin.Users.Permissions.Get.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}