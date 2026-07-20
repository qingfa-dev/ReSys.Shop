using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Create;

public static partial class CreateOrder
{
    /// <summary>Maps the admin order-creation route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/ordering/orders — admin create a new order
            app.MapPost(OrderingFeature.Admin.Orders.Create.Route, async ([FromBody] Request request, ISender sender, CancellationToken ct) =>
            {
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