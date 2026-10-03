namespace Module.Identity.Features.Shared.Storefront.Passwords.Forgot;

public static partial class RequestPasswordReset
{
    /// <summary>Maps the password reset request route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/storefront/passwords/forgot — send password reset email
            app.MapPost(IdentityFeature.Storefront.Passwords.Forgot.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .RequireRateLimiting("forgot-password")
            .WithName(nameof(RequestPasswordReset))
            .WithTags(IdentityFeature.Tags.Authentication)
            .WithSummary(IdentityFeature.Storefront.Passwords.Forgot.Summary)
            .WithDescription(IdentityFeature.Storefront.Passwords.Forgot.Description)
            .Produces<Result>(StatusCodes.Status204NoContent)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}