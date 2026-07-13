namespace Module.Identity.Features.Admin.Users.Roles.Get;

public static partial class GetUserRoles
{
    /// <summary>
    /// Represents the API endpoint for retrieving roles associated with a specific user.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
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