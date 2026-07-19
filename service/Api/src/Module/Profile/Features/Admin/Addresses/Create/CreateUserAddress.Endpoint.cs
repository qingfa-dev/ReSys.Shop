using Module.Profile.Features.Shared;
using Module.Profile.Features.Store.Addresses.Create;

namespace Module.Profile.Features.Admin.Addresses.Create;

public static partial class CreateUserAddress
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(ProfileFeature.Admin.Addresses.Create.Route, async (
                [FromBody] CreateAddress.Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new CreateAddress.Command(request.UserId, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(CreateUserAddress))
            .WithTags(ProfileFeature.Tags.Address)
            .WithSummary(ProfileFeature.Admin.Addresses.Create.Summary)
            .WithDescription(ProfileFeature.Admin.Addresses.Create.Description)
            .Produces<Result<CreateAddress.Response>>(StatusCodes.Status201Created)
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
