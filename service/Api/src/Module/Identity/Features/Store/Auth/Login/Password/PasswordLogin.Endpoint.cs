namespace Module.Identity.Features.Store.Auth.Login.Password;

public static partial class PasswordLogin
{
    /// <summary>Maps the password-based login route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/store/auth/login/password — authenticate with email and password
            app.MapPost(IdentityFeature.Store.Auth.Login.Password.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(PasswordLogin))
            .WithTags(IdentityFeature.Tags.Authentication)
            .AllowAnonymous()
            .WithSummary(IdentityFeature.Store.Auth.Login.Password.Summary)
            .WithDescription(IdentityFeature.Store.Auth.Login.Password.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}