namespace Module.Identity.Features.Shared.Admin.Users.GetById;

public static partial class GetUserById
{
    /// <summary>Maps the user detail route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/admin/users/{id} — single user by ID
            app.MapGet(IdentityFeature.Admin.Users.GetById.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetUserById))
            .WithTags(IdentityFeature.Tags.User)
            .HasPermission(IdentityFeature.Admin.Users.GetById.Permission)
            .WithSummary(IdentityFeature.Admin.Users.GetById.Summary)
            .WithDescription(IdentityFeature.Admin.Users.GetById.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}