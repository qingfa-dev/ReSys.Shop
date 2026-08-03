namespace Module.Identity.Features.Storefront.Passwords.Reset;

public static partial class ResetPassword
{
    /// <summary>Maps the password reset finalisation route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/store/passwords/reset — finalise password reset with token
            app.MapPost(IdentityFeature.Store.Passwords.Reset.Route, async (
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
            .WithSummary(IdentityFeature.Store.Passwords.Reset.Summary)
            .WithDescription(IdentityFeature.Store.Passwords.Reset.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}