using Module.Profile.Features.Shared;

namespace Module.Profile.Features.Store.Profile.Get.Detail;

public static partial class GetProfile
{
    /// <summary>
    /// Represents the API endpoint for retrieving the authenticated user's profile.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for retrieving a profile to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a GET endpoint for retrieving the authenticated user's profile.
            app.MapGet(ProfileFeature.Store.Profile.Get.Route, async (
                ISender sender,
                ICurrentUser currentUser,
                CancellationToken ct) =>
            {
                // Guard: Reject unauthenticated requests.
                if (!currentUser.IsAuthenticated || string.IsNullOrEmpty(currentUser.UserId))
                    return Results.Unauthorized();

                // Create: Construct a query from the current user's identity.
                if (!Guid.TryParse(currentUser.UserId, out var userId))
                    return Results.Unauthorized();
                var query = new Query(userId);
                // Send: Dispatch the query to the mediator for processing.
                var result = await sender.Send(query, ct);
                // Map: Convert the result to an IResult for the HTTP response.
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(GetProfile))
            .WithTags(ProfileFeature.Tags.Profile)
            .WithSummary(ProfileFeature.Store.Profile.Get.Summary)
            .WithDescription(ProfileFeature.Store.Profile.Get.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
