using Carter;
using MediatR;
using Module.Catalog.Features.Shared;
using Shared.Application.Extensions.Results;
using Shared.Security.Authorization.Attributes;

namespace Module.Catalog.Features.Admin.Dashboard.Get;

public static partial class GetCatalogDashboard
{
    /// <summary>Maps the catalog admin dashboard route.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /api/admin/catalog/dashboard — catalog stats for admin dashboard
            app.MapGet(CatalogDashboardFeature.Admin.Get.Route, async (
                ISender sender,
                CancellationToken ct) =>
            {
                var query = new Query();
                var result = await sender.Send(query, ct);
                return result.ToResult();
            })
            .WithName(nameof(GetCatalogDashboard))
            .WithTags(CatalogDashboardFeature.Tags.Catalog)
            .HasPermission(CatalogDashboardFeature.Admin.Get.Permission)
            .WithSummary(CatalogDashboardFeature.Admin.Get.Summary)
            .WithDescription(CatalogDashboardFeature.Admin.Get.Description)
            .Produces<Result<Response>>();
        }
    }
}
