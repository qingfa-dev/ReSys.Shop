using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Orders.Resume;

public static partial class ResumeOrder
{
    /// <summary>Maps the admin order-resumption route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: POST api/ordering/orders/{id:guid}/resume — resume a canceled order
            app.MapPost(OrderingFeature.Admin.Orders.Resume.Route, async ([FromRoute] Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(id), ct);
                return result.ToResult();
            })
            .WithName(nameof(ResumeOrder))
            .WithTags(OrderingFeature.Tags.Order)
            .HasPermission(OrderingFeature.Admin.Orders.Resume.Permission)
            .WithSummary(OrderingFeature.Admin.Orders.Resume.Summary)
            .WithDescription(OrderingFeature.Admin.Orders.Resume.Description)
            .Produces<Result<Response>>()
            .Produces<Result>(StatusCodes.Status400BadRequest)
            .Produces<Result>(StatusCodes.Status404NotFound);
        }
    }
}