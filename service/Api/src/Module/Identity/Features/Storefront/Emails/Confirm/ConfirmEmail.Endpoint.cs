namespace Module.Identity.Features.Storefront.Emails.Confirm;

public static partial class ConfirmEmail
{
    /// <summary>Maps the email confirmation route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/store/emails/confirm — confirm email or finalise email change
            app.MapPost(IdentityFeature.Store.Emails.Confirm.Route, Handle)
                .AllowAnonymous()
                .WithName(nameof(ConfirmEmail))
                .WithTags(IdentityFeature.Tags.Authentication)
                .WithSummary(IdentityFeature.Store.Emails.Confirm.Summary)
                .WithDescription(IdentityFeature.Store.Emails.Confirm.Description)
                .Produces<Result>()
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status204NoContent);
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