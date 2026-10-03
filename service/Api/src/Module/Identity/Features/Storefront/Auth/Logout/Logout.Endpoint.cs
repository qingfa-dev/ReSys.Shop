namespace Module.Identity.Features.Shared.Storefront.Auth.Logout;

public static partial class Logout
{
    /// <summary>Maps the logout route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/storefront/auth/logout — revoke refresh token and sign out
            app.MapPost(IdentityFeature.Storefront.Auth.Logout.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(Logout))
            .WithTags(IdentityFeature.Tags.Authentication)
            .WithSummary(IdentityFeature.Storefront.Auth.Logout.Summary)
            .WithDescription(IdentityFeature.Storefront.Auth.Logout.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}