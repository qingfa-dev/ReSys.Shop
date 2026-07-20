namespace Module.Identity.Features.Admin.Users.Create;

public static partial class CreateUser
{
    /// <summary>Maps the user creation route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/admin/users — create a new user
            app.MapPost(IdentityFeature.Admin.Users.Create.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateUser))
            .WithTags(IdentityFeature.Tags.User)
            .HasPermission(IdentityFeature.Admin.Users.Create.Permission)
            .WithSummary(IdentityFeature.Admin.Users.Create.Summary)
            .WithDescription(IdentityFeature.Admin.Users.Create.Description)
            .Produces<Result<Response>>(StatusCodes.Status201Created)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status409Conflict)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity)
            .Produces<Result<Response>>(StatusCodes.Status404NotFound);
        }
    }
}