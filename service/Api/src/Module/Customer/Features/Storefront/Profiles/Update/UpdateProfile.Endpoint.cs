using Module.Customer.Features.Shared;

namespace Module.Customer.Features.Storefront.Profiles.Update;

public static partial class UpdateProfile
{
    /// <summary>
    /// Represents the API endpoint for updating the authenticated user's profile.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        /// <summary>
        /// Adds the endpoint for updating a profile to the Carter module.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: Defines a PATCH endpoint for updating the authenticated user's profile.
            app.MapPatch(ProfileFeature.Storefront.Profiles.Update.Route, async (
                [FromBody] Request request,
                ISender sender,
                ICurrentUser currentUser,
                CancellationToken ct) =>
            {
                // Create: Construct a command wrapping the request body.
                if (!Guid.TryParse(currentUser.UserId, out var userId))
                    return Results.Unauthorized();
                var command = new Command(userId, request);
                // Send: Dispatch the command to the mediator for processing.
                var result = await sender.Send(command, ct);
                // Map: Convert the result to an IResult for the HTTP response.
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(UpdateProfile))
            .WithTags(ProfileFeature.Tags.Profile)
            .WithSummary(ProfileFeature.Storefront.Profiles.Update.Summary)
            .WithDescription(ProfileFeature.Storefront.Profiles.Update.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}