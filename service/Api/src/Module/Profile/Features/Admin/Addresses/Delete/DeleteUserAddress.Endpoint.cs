using Module.Profile.Features.Shared;
using Module.Profile.Features.Store.Addresses.Delete;

namespace Module.Profile.Features.Admin.Addresses.Delete;

public static partial class DeleteUserAddress
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(ProfileFeature.Admin.Addresses.Delete.Route, async (
                [FromRoute] Guid id,
                [FromQuery] Guid userId,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new DeleteAddress.Command(userId, id);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(DeleteUserAddress))
            .WithTags(ProfileFeature.Tags.Address)
            .WithSummary(ProfileFeature.Admin.Addresses.Delete.Summary)
            .WithDescription(ProfileFeature.Admin.Addresses.Delete.Description)
            .Produces<Result<DeleteAddress.Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
