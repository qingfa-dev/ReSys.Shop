namespace Module.Identity.Features.Store.Emails.Change;

public static partial class ChangeEmail
{
    /// <summary>
    /// Carter endpoint for initiating an email change request.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(IdentityFeature.Store.Emails.Change.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                // Handle: Initiate email change — routes Command to handler via MediatR
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(ChangeEmail))
            .WithTags(IdentityFeature.Tags.Authentication)
            .WithSummary(IdentityFeature.Store.Emails.Change.Summary)
            .WithDescription(IdentityFeature.Store.Emails.Change.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
