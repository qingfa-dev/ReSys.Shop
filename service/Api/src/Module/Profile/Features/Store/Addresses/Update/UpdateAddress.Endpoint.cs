using Module.Profile.Features.Shared;

namespace Module.Profile.Features.Store.Addresses.Update;

public static partial class UpdateAddress
{
    // ============ ENDPOINT ============
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(ProfileFeature.Store.Addresses.Update.Route, async (
                    [FromRoute] Guid id,
                    [FromBody] Request request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new Command(id, request);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToResult();
                })
                .RequireAuthorization()
                .WithName(nameof(UpdateAddress))
                .WithTags(ProfileFeature.Tags.Address)
                .WithSummary(ProfileFeature.Store.Addresses.Update.Summary)
                .WithDescription(ProfileFeature.Store.Addresses.Update.Description)
                .Produces<Result<Response>>()
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status401Unauthorized)
                .Produces<Result>(StatusCodes.Status404NotFound)
                .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}