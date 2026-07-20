namespace Module.Identity.Features.Admin.Users.Roles.Get;

public static partial class GetUserRoles
{
    /// <summary>Maps the user role retrieval route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/admin/users/{id}/roles — get roles assigned to a user
            app.MapGet(IdentityFeature.Admin.Users.Roles.Get.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .HasPermission(IdentityFeature.Admin.Users.Roles.Get.Permission)
            .WithName(nameof(GetUserRoles))
            .WithTags(IdentityFeature.Tags.User)
            .WithSummary(IdentityFeature.Admin.Users.Roles.Get.Summary)
            .WithDescription(IdentityFeature.Admin.Users.Roles.Get.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}