namespace Module.Identity.Features.Store.Emails.Confirm;

public static partial class ConfirmEmail
{
    /// <summary>
    /// Carter endpoint for email confirmation and email change finalisation.
    /// </summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(IdentityFeature.Store.Emails.Confirm.Route, Handle)
                .AllowAnonymous()
                .WithName(nameof(ConfirmEmail))
                .WithTags(IdentityFeature.Tags.Authentication)
                .WithSummary(IdentityFeature.Store.Emails.Confirm.Summary)
                .WithDescription(IdentityFeature.Store.Emails.Confirm.Description)
                .Produces<Result>()
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status404NotFound);
        }

        // Handle: Confirm email or finalise email change — routes Command to handler via MediatR
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
