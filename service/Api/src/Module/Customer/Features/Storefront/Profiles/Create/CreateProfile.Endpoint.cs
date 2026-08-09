using Module.Customer.Features.Shared;

using Shared.Security.Identity.Domain.Users;

namespace Module.Customer.Features.Storefront.Profiles.Create;

public static partial class CreateProfile
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(ProfileFeature.Storefront.Profiles.Create.Route, async (
                    [FromBody] Request request,
                    ISender sender,
                    ICurrentUser currentUser,
                    CancellationToken cancellationToken) =>
                {
                    if (string.IsNullOrEmpty(currentUser.UserId))
                        return Results.Unauthorized();

                    var command = new Command(Guid.Parse(currentUser.UserId), request);
                    var result = await sender.Send(command, cancellationToken);
                    return result.ToResult();
                })
                .RequireAuthorization()
                .WithName(nameof(CreateProfile))
                .WithTags(ProfileFeature.Tags.Profile)
                .WithSummary(ProfileFeature.Storefront.Profiles.Create.Summary)
                .WithDescription(ProfileFeature.Storefront.Profiles.Create.Description)
                .Produces<Result<Response>>(StatusCodes.Status201Created)
                .Produces<Result>(StatusCodes.Status400BadRequest)
                .Produces<Result>(StatusCodes.Status401Unauthorized)
                .Produces<Result>(StatusCodes.Status404NotFound)
                .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
