namespace Module.Identity.Features.Admin.Roles.Create;

public static partial class CreateRole
{
    /// <summary>Maps the role creation route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/admin/roles — create a new role
            app.MapPost(IdentityFeature.Admin.Roles.Create.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateRole))
            .WithTags(IdentityFeature.Tags.Role)
            .HasPermission(IdentityFeature.Admin.Roles.Create.Permission)
            .WithSummary(IdentityFeature.Admin.Roles.Create.Summary)
            .WithDescription(IdentityFeature.Admin.Roles.Create.Description)
            .Produces<Result<Response>>(StatusCodes.Status201Created)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status409Conflict)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}