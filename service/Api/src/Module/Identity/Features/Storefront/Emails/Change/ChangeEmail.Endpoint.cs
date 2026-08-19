namespace Module.Identity.Features.Shared.Storefront.Emails.Change;

public static partial class ChangeEmail
{
    /// <summary>Maps the email change initiation route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/storefront/emails/change — initiate email address change
            app.MapPost(IdentityFeature.Storefront.Emails.Change.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(ChangeEmail))
            .WithTags(IdentityFeature.Tags.Authentication)
            .WithSummary(IdentityFeature.Storefront.Emails.Change.Summary)
            .WithDescription(IdentityFeature.Storefront.Emails.Change.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}