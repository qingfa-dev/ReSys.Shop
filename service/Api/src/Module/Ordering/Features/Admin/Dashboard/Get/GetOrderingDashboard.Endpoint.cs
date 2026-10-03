using Module.Ordering.Features.Shared;

namespace Module.Ordering.Features.Admin.Dashboard.Get;

public static partial class GetOrderingDashboard
{
    /// <summary>Maps the admin ordering dashboard route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET api/admin/ordering/dashboard — get ordering dashboard metrics
            app.MapGet(OrderingDashboardFeature.Admin.Get.Route, async (
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query();
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetOrderingDashboard))
            .WithTags(OrderingDashboardFeature.Tags.Ordering)
            .HasPermission(OrderingDashboardFeature.Admin.Get.Permission)
            .WithSummary(OrderingDashboardFeature.Admin.Get.Summary)
            .WithDescription(OrderingDashboardFeature.Admin.Get.Description)
            .Produces<Result<Response>>();
        }
    }
}
