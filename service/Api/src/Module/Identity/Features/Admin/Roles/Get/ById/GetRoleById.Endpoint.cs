namespace Module.Identity.Features.Shared.Admin.Roles.Get.ById;

public static partial class GetRoleById
{
    /// <summary>Maps the role detail route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/admin/roles/{id} — single role by ID
            app.MapGet(IdentityFeature.Admin.Roles.GetById.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetRoleById))
            .WithTags(IdentityFeature.Tags.Role)
            .HasPermission(IdentityFeature.Admin.Roles.GetById.Permission)
            .WithSummary(IdentityFeature.Admin.Roles.GetById.Summary)
            .WithDescription(IdentityFeature.Admin.Roles.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}