namespace Module.Identity.Features.Shared.Admin.Users.GetPagedOrAll;

public static partial class GetUsersPagedOrAll
{
    /// <summary>Maps the user listing route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/admin/users — paged user listing with filtering and sorting
            app.MapGet(IdentityFeature.Admin.Users.GetAll.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(parameters);
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetUsersPagedOrAll))
            .WithTags(IdentityFeature.Tags.User)
            .HasPermission(IdentityFeature.Admin.Users.GetAll.Permission)
            .WithSummary(IdentityFeature.Admin.Users.GetAll.Summary)
            .WithDescription(IdentityFeature.Admin.Users.GetAll.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<PagedResult<Response>>(StatusCodes.Status404NotFound);
        }
    }
}