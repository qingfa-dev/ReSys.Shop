namespace Module.Identity.Features.Shared.Storefront.Emails.Resend;

public static partial class ResendEmailVerification
{
    /// <summary>Maps the email verification resend route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/storefront/emails/resend — resend email verification link
            app.MapPost(IdentityFeature.Storefront.Emails.Resend.Route, Handle)
                .AllowAnonymous()
                .WithName(nameof(ResendEmailVerification))
                .WithTags(IdentityFeature.Tags.Authentication)
                .WithSummary(IdentityFeature.Storefront.Emails.Resend.Summary)
                .WithDescription(IdentityFeature.Storefront.Emails.Resend.Description)
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