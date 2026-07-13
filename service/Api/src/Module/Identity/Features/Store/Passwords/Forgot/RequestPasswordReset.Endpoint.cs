namespace Module.Identity.Features.Store.Passwords.Forgot;

public static partial class RequestPasswordReset
{
    /// <summary>
    /// Carter endpoint for requesting a password reset link.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(IdentityFeature.Store.Passwords.Forgot.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                // Handle: Request password reset — routes Command to handler via MediatR
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