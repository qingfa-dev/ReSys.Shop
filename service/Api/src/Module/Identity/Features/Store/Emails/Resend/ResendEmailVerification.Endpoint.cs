namespace Module.Identity.Features.Store.Emails.Resend;

public static partial class ResendEmailVerification
{
    /// <summary>
    /// Carter endpoint for resending email verification links.
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
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

        // Handle: Resend email verification — routes Command to handler via MediatR
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
