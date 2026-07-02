namespace Module.Identity.Features.Store.Passwords.Reset;

public static partial class ResetPassword
{
    /// <summary>
    /// Carter endpoint for finalising a password reset.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(IdentityFeature.Store.Passwords.Reset.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                // Handle: Reset password — routes Command to handler via MediatR
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
