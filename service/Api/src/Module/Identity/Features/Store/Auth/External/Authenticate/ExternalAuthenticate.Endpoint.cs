namespace Module.Identity.Features.Store.Auth.External.Authenticate;

public static partial class ExternalAuthenticate
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(IdentityFeature.Store.Auth.Login.External.Authenticate.Route, async (
                    [FromBody] Request request,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    var command = new Command(request);
                    var result = await sender.Send(command, ct);
                    return result.ToResult();
                })
                .WithName(nameof(ExternalAuthenticate))
                .WithTags(IdentityFeature.Tags.Authentication)
                .AllowAnonymous()
                .WithSummary(IdentityFeature.Store.Auth.Login.External.Authenticate.Summary)
                .WithDescription(IdentityFeature.Store.Auth.Login.External.Authenticate.Description)
                .Produces<Result<Response>>()
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status401Unauthorized)
                .Produces<Result>(StatusCodes.Status404NotFound)
                .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}