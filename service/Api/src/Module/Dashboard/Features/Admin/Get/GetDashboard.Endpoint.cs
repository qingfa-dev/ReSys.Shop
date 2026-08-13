using Module.Dashboard.Features.Shared;

namespace Module.Dashboard.Features.Admin.Get;

public static partial class GetDashboard
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(DashboardFeature.Admin.Get.Route, async (
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query();
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetDashboard))
            .WithTags(DashboardFeature.Tags.Dashboard)
            .HasPermission(DashboardFeature.Admin.Get.Permission)
            .WithSummary(DashboardFeature.Admin.Get.Summary)
            .WithDescription(DashboardFeature.Admin.Get.Description)
            .Produces<Result<Response>>();
        }
    }
}
