namespace Module.Identity.Features.Store.Passwords.Forgot;

public static partial class RequestPasswordReset
{
    /// <summary>Maps the password reset request route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/store/passwords/forgot — send password reset email
            app.MapPost(IdentityFeature.Store.Passwords.Forgot.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .WithName(nameof(RequestPasswordReset))
            .WithTags(IdentityFeature.Tags.Authentication)
            .WithSummary(IdentityFeature.Store.Passwords.Forgot.Summary)
            .WithDescription(IdentityFeature.Store.Passwords.Forgot.Description)
            .Produces<Result>(StatusCodes.Status204NoContent)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}