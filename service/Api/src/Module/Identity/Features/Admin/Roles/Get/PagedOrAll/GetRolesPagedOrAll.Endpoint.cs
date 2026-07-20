namespace Module.Identity.Features.Admin.Roles.Get.PagedOrAll;

public static partial class GetRolesPagedOrAll
{
    /// <summary>Maps the role listing route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/admin/roles — paged role listing with filtering and sorting
            app.MapGet(IdentityFeature.Admin.Roles.GetAll.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetRolesPagedOrAll))
            .WithTags(IdentityFeature.Tags.Role)
            .HasPermission(IdentityFeature.Admin.Roles.GetAll.Permission)
            .WithSummary(IdentityFeature.Admin.Roles.GetAll.Summary)
            .WithDescription(IdentityFeature.Admin.Roles.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<PagedResult<Response>>(StatusCodes.Status404NotFound);
        }
    }
}