namespace Module.Identity.Features.Shared.Storefront.Auth.Register;

public static partial class EmailRegister
{
    /// <summary>Maps the email registration route.</summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST /api/storefront/auth/register — register a new account with email and password
            app.MapPost(IdentityFeature.Storefront.Auth.Register.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .AllowAnonymous()
            .RequireRateLimiting("register")
            .WithName(nameof(EmailRegister))
            .WithTags(IdentityFeature.Tags.Authentication)
            .WithSummary(IdentityFeature.Storefront.Auth.Register.Summary)
            .WithDescription(IdentityFeature.Storefront.Auth.Register.Description)
            .Produces<Result<Response>>(StatusCodes.Status201Created)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}