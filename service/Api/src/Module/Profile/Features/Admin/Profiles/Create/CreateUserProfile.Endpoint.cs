using Module.Profile.Features.Shared;
using Module.Profile.Features.Storefront.Profiles.Create;

namespace Module.Profile.Features.Admin.Profiles.CreateUserProfile;

public static partial class CreateUserProfile
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(ProfileFeature.Admin.Profiles.Create.Route, async (
                [FromBody] CreateProfile.Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new CreateProfile.Command(request.UserId, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateUserProfile))
            .WithTags(ProfileFeature.Tags.Profile)
            .HasPermission(ProfileFeature.Admin.Profiles.Create.Permission)
            .WithSummary(ProfileFeature.Admin.Profiles.Create.Summary)
            .WithDescription(ProfileFeature.Admin.Profiles.Create.Description)
            .Produces<Result<CreateProfile.Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
