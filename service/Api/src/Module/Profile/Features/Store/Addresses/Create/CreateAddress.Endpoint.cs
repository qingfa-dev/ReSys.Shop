using Module.Profile.Features.Shared;

namespace Module.Profile.Features.Store.Addresses.Create;

public static partial class CreateAddress
{
    // ============ ENDPOINT ============
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(ProfileFeature.Store.Addresses.Create.Route, async (
                    [FromBody] Request request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new Command(request);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToResult();
                })
                .RequireAuthorization()
                .WithName(nameof(CreateAddress))
                .WithTags(ProfileFeature.Tags.Address)
                .WithSummary(ProfileFeature.Store.Addresses.Create.Summary)
                .WithDescription(ProfileFeature.Store.Addresses.Create.Description)
                .Produces<Result<Response>>(StatusCodes.Status201Created)
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status401Unauthorized)
                .Produces<Result>(StatusCodes.Status404NotFound)
                .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}