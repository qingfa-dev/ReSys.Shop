namespace Module.Identity.Features.Storefront.Emails.Resend;

public static partial class ResendEmailVerification
{
    /// <summary>Maps the email verification resend route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/store/emails/resend — resend email verification link
            app.MapPost(IdentityFeature.Store.Emails.Resend.Route, Handle)
                .AllowAnonymous()
                .WithName(nameof(ResendEmailVerification))
                .WithTags(IdentityFeature.Tags.Authentication)
                .WithSummary(IdentityFeature.Store.Emails.Resend.Summary)
                .WithDescription(IdentityFeature.Store.Emails.Resend.Description)
                .Produces<Result>()
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status404NotFound);
        }

        private static async Task<IResult> Handle(
            [FromBody] Request request,
            ISender sender,
            CancellationToken ct)
        {
            var command = new Command(request);
            var result = await sender.Send(command, ct);
            return result.ToResult();
        }
    }
}