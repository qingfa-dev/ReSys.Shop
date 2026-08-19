namespace Module.Identity.Features.Shared.Storefront.Passwords.Reset;

public static partial class ResetPassword
{
    /// <summary>Maps the password reset finalisation route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/storefront/passwords/reset — finalise password reset with token
            app.MapPost(IdentityFeature.Storefront.Passwords.Reset.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(ResetPassword))
            .WithTags(IdentityFeature.Tags.Authentication)
            .WithSummary(IdentityFeature.Storefront.Passwords.Reset.Summary)
            .WithDescription(IdentityFeature.Storefront.Passwords.Reset.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}