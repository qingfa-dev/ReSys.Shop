using Module.Profile.Features.Shared;

namespace Module.Profile.Features.Admin.Profiles.UpdateUserProfile;

public static partial class UpdateUserProfile
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(ProfileFeature.Admin.Profiles.Update.Route, async (
                [FromBody] Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new Command(request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(UpdateUserProfile))
            .WithTags(ProfileFeature.Tags.Profile)
            .HasPermission(ProfileFeature.Admin.Profiles.Update.Permission)
            .WithSummary(ProfileFeature.Admin.Profiles.Update.Summary)
            .WithDescription(ProfileFeature.Admin.Profiles.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
