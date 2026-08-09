using Module.Customer.Features.Shared;
using Module.Customer.Features.Storefront.Addresses.Update;

namespace Module.Customer.Features.Admin.Addresses.Update;

public static partial class UpdateUserAddress
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(ProfileFeature.Admin.Addresses.Update.Route, async (
                [FromRoute] Guid id,
                [FromBody] UpdateAddress.Request request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new UpdateAddress.Command(request.UserId, id, request);
                var result = await sender.Send(command, ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .WithName(nameof(UpdateUserAddress))
            .WithTags(ProfileFeature.Tags.Address)
            .WithSummary(ProfileFeature.Admin.Addresses.Update.Summary)
            .WithDescription(ProfileFeature.Admin.Addresses.Update.Description)
            .Produces<Result<UpdateAddress.Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}
