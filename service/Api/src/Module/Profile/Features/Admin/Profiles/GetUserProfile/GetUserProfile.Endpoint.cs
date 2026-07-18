using Module.Profile.Features.Shared;

namespace Module.Profile.Features.Admin.Profiles.GetUserProfile;

public static partial class GetUserProfile
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(ProfileFeature.Admin.Profiles.Get.Route, async (
                [FromQuery] Guid userId,
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query(userId);
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetUserProfile))
            .WithTags(ProfileFeature.Tags.Profile)
            .HasPermission(ProfileFeature.Admin.Profiles.Get.Permission)
            .WithSummary(ProfileFeature.Admin.Profiles.Get.Summary)
            .WithDescription(ProfileFeature.Admin.Profiles.Get.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
