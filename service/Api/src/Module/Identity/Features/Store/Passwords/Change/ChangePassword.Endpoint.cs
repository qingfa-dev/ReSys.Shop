namespace Module.Identity.Features.Store.Passwords.Change;

public static partial class ChangePassword
{
    /// <summary>
    /// Carter endpoint for authenticated password change.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(IdentityFeature.Store.Passwords.Change.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                // Handle: Change password — routes Command to handler via MediatR
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(ChangePassword))
            .WithTags(IdentityFeature.Tags.Authentication)
            .WithSummary(IdentityFeature.Store.Passwords.Change.Summary)
            .WithDescription(IdentityFeature.Store.Passwords.Change.Description)
            .Produces<Result>(StatusCodes.Status202Accepted)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
