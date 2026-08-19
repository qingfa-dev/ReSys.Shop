namespace Module.Identity.Features.Shared.Admin.Roles.Delete;

public static partial class DeleteRole
{
    /// <summary>Maps the role deletion route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: DELETE /api/admin/roles/{id} — delete a role
            app.MapDelete(IdentityFeature.Admin.Roles.Delete.Route, async (
                Guid id,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(new Request { Id = id });
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteRole))
            .WithTags(IdentityFeature.Tags.Role)
            .HasPermission(IdentityFeature.Admin.Roles.Delete.Permission)
            .WithSummary(IdentityFeature.Admin.Roles.Delete.Summary)
            .WithDescription(IdentityFeature.Admin.Roles.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status403Forbidden)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}