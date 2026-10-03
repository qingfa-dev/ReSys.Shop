namespace Module.Identity.Features.Shared.Storefront.Auth.Login.External.Authenticate;

public static partial class ExternalAuthenticate
{
    /// <summary>Maps the external OAuth authentication route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/storefront/auth/login/external — authenticate via external OAuth provider (Google, Facebook)
            app.MapPost(IdentityFeature.Storefront.Auth.Login.External.Authenticate.Route, async (
                    [FromBody] Request request,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    var command = new Command(request);
                    var result = await sender.Send(command, ct);
                    return result.ToResult();
                })
                .WithName(nameof(ExternalAuthenticate))
                .WithTags(IdentityFeature.Tags.Authentication)
                .AllowAnonymous()
                .RequireRateLimiting("auth")
                .WithSummary(IdentityFeature.Storefront.Auth.Login.External.Authenticate.Summary)
                .WithDescription(IdentityFeature.Storefront.Auth.Login.External.Authenticate.Description)
                .Produces<Result<Response>>()
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status401Unauthorized)
                .Produces<Result>(StatusCodes.Status404NotFound)
                .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}