using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Create;
public static partial class CreateOrder
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(OrderingFeature.Admin.Orders.Create.Route, async ([FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
                // Call: Dispatch CreateOrder command via MediatR.
                var result = await sender.Send(new Command(request), ct);
                return result.ToResult();
            })
            .WithName(nameof(CreateOrder))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.Create.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.Create.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.Create.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest);
        }
    }
}
