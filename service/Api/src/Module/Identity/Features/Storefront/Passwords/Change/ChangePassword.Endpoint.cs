namespace Module.Identity.Features.Shared.Storefront.Passwords.Change;

public static partial class ChangePassword
{
    /// <summary>Maps the authenticated password change route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/storefront/passwords/change — change password for authenticated user
            app.MapPost(IdentityFeature.Storefront.Passwords.Change.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(ChangePassword))
            .WithTags(IdentityFeature.Tags.Authentication)
            .WithSummary(IdentityFeature.Storefront.Passwords.Change.Summary)
            .WithDescription(IdentityFeature.Storefront.Passwords.Change.Description)
            .Produces<Result>(StatusCodes.Status202Accepted)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}