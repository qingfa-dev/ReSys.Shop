namespace Module.Identity.Features.Shared.Storefront.Auth.Sessions.Refresh;

public static partial class RefreshSession
{
    /// <summary>Maps the session refresh route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/storefront/auth/sessions/refresh — refresh an expired JWT access token
            app.MapPost(IdentityFeature.Storefront.Auth.Sessions.Refresh.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(RefreshSession))
            .WithTags(IdentityFeature.Tags.Authentication)
            .AllowAnonymous()
            .WithSummary(IdentityFeature.Storefront.Auth.Sessions.Refresh.Summary)
            .WithDescription(IdentityFeature.Storefront.Auth.Sessions.Refresh.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}