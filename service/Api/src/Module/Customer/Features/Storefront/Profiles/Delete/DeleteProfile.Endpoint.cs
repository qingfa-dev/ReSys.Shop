using Module.Customer.Features.Shared;

namespace Module.Customer.Features.Storefront.Profiles.Delete;

public static partial class DeleteProfile
{
    public sealed class Endpoint : ICarterModule
    {
        // Route: DELETE api/storefront/identity/profiles — deactivate authenticated user's profile
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(ProfileFeature.Storefront.Profiles.Delete.Route, async (
                ISender sender,
                ICurrentUser currentUser,
                CancellationToken ct) =>
            {
                if (!Guid.TryParse(currentUser.UserId, out var userId))
                    return Results.Unauthorized();
                var command = new Command(userId);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(DeleteProfile))
            .WithTags(ProfileFeature.Tags.Profile)
            .WithSummary(ProfileFeature.Storefront.Profiles.Delete.Summary)
            .WithDescription(ProfileFeature.Storefront.Profiles.Delete.Description)
            .Produces<Result>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}