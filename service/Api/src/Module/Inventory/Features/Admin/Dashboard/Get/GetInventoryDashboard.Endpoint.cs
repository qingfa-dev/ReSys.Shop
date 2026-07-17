using Carter;
using MediatR;
using Module.Inventory.Features.Shared;
using Shared.Application.Extensions.Results;
using Shared.Security.Authorization.Attributes;

namespace Module.Inventory.Features.Admin.Dashboard.Get;

public static partial class GetInventoryDashboard
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(InventoryDashboardFeature.Admin.Get.Route, async (
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query();
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetInventoryDashboard))
            .WithTags(InventoryDashboardFeature.Tags.Inventory)
            .HasPermission(InventoryDashboardFeature.Admin.Get.Permission)
            .WithSummary(InventoryDashboardFeature.Admin.Get.Summary)
            .WithDescription(InventoryDashboardFeature.Admin.Get.Description)
            .Produces<Result<Response>>();
        }
    }
}
