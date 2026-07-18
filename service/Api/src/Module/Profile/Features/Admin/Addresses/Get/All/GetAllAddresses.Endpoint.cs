using Module.Profile.Features.Shared;

namespace Module.Profile.Features.Admin.Addresses.Get.All;

public static partial class GetAllAddresses
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ProfileFeature.Admin.Addresses.GetAll.Route, async (
                [FromQuery] Guid userId,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(userId);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(GetAllAddresses))
            .WithTags(ProfileFeature.Tags.Address)
            .WithSummary(ProfileFeature.Admin.Addresses.GetAll.Summary)
            .WithDescription(ProfileFeature.Admin.Addresses.GetAll.Description)
            .Produces<Result<List<Response>>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
