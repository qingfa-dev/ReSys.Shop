namespace Module.Identity.Features.Admin.Users.Status;

public static partial class ToggleUserStatus
{
    /// <summary>Maps the user status toggle route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: PATCH /api/admin/users/{id}/status — toggle active/inactive status
            app.MapPatch(IdentityFeature.Admin.Users.Status.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(ToggleUserStatus))
            .WithTags(IdentityFeature.Tags.User)
            .HasPermission(IdentityFeature.Admin.Users.Status.Permission)
            .WithSummary(IdentityFeature.Admin.Users.Status.Summary)
            .WithDescription(IdentityFeature.Admin.Users.Status.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}