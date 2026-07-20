namespace Module.Identity.Features.Admin.Users.Delete;

public static partial class DeleteUser
{
    /// <summary>Maps the user deletion route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: DELETE /api/admin/users/{id} — delete a user
            app.MapDelete(IdentityFeature.Admin.Users.Delete.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(new Request { Id = id });
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteUser))
            .WithTags(IdentityFeature.Tags.User)
            .HasPermission(IdentityFeature.Admin.Users.Delete.Permission)
            .WithSummary(IdentityFeature.Admin.Users.Delete.Summary)
            .WithDescription(IdentityFeature.Admin.Users.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}