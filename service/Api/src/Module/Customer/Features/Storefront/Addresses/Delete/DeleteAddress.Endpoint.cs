using Module.Customer.Features.Shared;

namespace Module.Customer.Features.Storefront.Addresses.Delete;

public static partial class DeleteAddress
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(ProfileFeature.Storefront.Addresses.Delete.Route, async (
                [FromRoute] Guid id,
                ISender sender,
                ICurrentUser currentUser,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrEmpty(currentUser.UserId))
                    return Results.Unauthorized();

                var command = new Command(Guid.Parse(currentUser.UserId), id);
                var result = await sender.Send(command, cancellationToken);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(DeleteAddress))
            .WithTags(ProfileFeature.Tags.Address)
            .WithSummary(ProfileFeature.Storefront.Addresses.Delete.Summary)
            .WithDescription(ProfileFeature.Storefront.Addresses.Delete.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status401Unauthorized)
            .Produces<Result>(StatusCodes.Status404NotFound)
            .Produces<Result>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
