namespace Module.Identity.Features.Admin.Roles.Update;

public static partial class UpdateRole
{
    /// <summary>Maps the role update route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: PUT /api/admin/roles/{id} — update role details
            app.MapPut(IdentityFeature.Admin.Roles.Update.Route, async (
                Guid id,
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(Id: id, Request: request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateRole))
            .WithTags(IdentityFeature.Tags.Role)
            .HasPermission(IdentityFeature.Admin.Roles.Update.Permission)
            .WithSummary(IdentityFeature.Admin.Roles.Update.Summary)
            .WithDescription(IdentityFeature.Admin.Roles.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status403Forbidden)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status409Conflict)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}