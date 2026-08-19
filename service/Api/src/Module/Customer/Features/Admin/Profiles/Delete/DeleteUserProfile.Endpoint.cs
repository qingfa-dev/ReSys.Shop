using Module.Customer.Features.Shared;
using Module.Customer.Features.Storefront.Profiles.Delete;

namespace Module.Customer.Features.Admin.Profiles.DeleteUserProfile;

public static partial class DeleteUserProfile
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(ProfileFeature.Admin.Profiles.Delete.Route, async (
                [FromQuery] Guid userId,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new DeleteProfile.Command(userId);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(DeleteUserProfile))
            .WithTags(ProfileFeature.Tags.Profile)
            .HasPermission(ProfileFeature.Admin.Profiles.Delete.Permission)
            .WithSummary(ProfileFeature.Admin.Profiles.Delete.Summary)
            .WithDescription(ProfileFeature.Admin.Profiles.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
