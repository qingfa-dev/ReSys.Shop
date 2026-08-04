using Module.Profile.Features.Shared;
using Module.Profile.Features.Storefront.Addresses.Get.PagedOrAll;

namespace Module.Profile.Features.Admin.Addresses.Get.All;

public static partial class GetAllAddresses
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ProfileFeature.Admin.Addresses.GetAll.Route, async (
                [FromQuery] Guid userId,
                [AsParameters] GetAddresses.Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new GetAddresses.Query(parameters with { UserId = userId });
                var result = await sender.Send(query, ct);
                return result.ToPagedResult();
            })
            .RequireAuthorization()
            .WithName(nameof(GetAllAddresses))
            .WithTags(ProfileFeature.Tags.Address)
            .WithSummary(ProfileFeature.Admin.Addresses.GetAll.Summary)
            .WithDescription(ProfileFeature.Admin.Addresses.GetAll.Description)
            .Produces<PagedResult<GetAddresses.Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
