using Module.Profile.Features.Shared;
using Module.Profile.Features.Storefront.Addresses.Get.ById;

namespace Module.Profile.Features.Admin.Addresses.Get.ById;

public static partial class GetUserAddressById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ProfileFeature.Admin.Addresses.GetById.Route, async (
                [FromRoute] Guid id,
                [FromQuery] Guid userId,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetAddressById.Query(userId, id);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(GetUserAddressById))
            .WithTags(ProfileFeature.Tags.Address)
            .WithSummary(ProfileFeature.Admin.Addresses.GetById.Summary)
            .WithDescription(ProfileFeature.Admin.Addresses.GetById.Description)
            .Produces<Result<GetAddressById.Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
