using Module.Profile.Features.Shared;
using Module.Profile.Features.Store.Profiles.Update;

namespace Module.Profile.Features.Admin.Profiles.UpdateUserProfile;

public static partial class UpdateUserProfile
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(ProfileFeature.Admin.Profiles.Update.Route, async (
                [FromBody] UpdateProfile.Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new UpdateProfile.Command(request.UserId, request, IsAdminBypass: true);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateUserProfile))
            .WithTags(ProfileFeature.Tags.Profile)
            .HasPermission(ProfileFeature.Admin.Profiles.Update.Permission)
            .WithSummary(ProfileFeature.Admin.Profiles.Update.Summary)
            .WithDescription(ProfileFeature.Admin.Profiles.Update.Description)
            .Produces<Result<UpdateProfile.Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
