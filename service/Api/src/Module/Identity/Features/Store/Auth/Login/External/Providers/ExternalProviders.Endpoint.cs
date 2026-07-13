namespace Module.Identity.Features.Store.Auth.Login.External.Providers;

public static partial class ExternalProviders
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(IdentityFeature.Store.Auth.Login.External.Providers.Route, async (
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new PagedQuery();
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(ExternalProviders))
            .WithTags(IdentityFeature.Tags.Authentication)
            .AllowAnonymous()
            .WithSummary(IdentityFeature.Store.Auth.Login.External.Providers.Summary)
            .WithDescription(IdentityFeature.Store.Auth.Login.External.Providers.Description)
            .Produces<PagedResult<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}