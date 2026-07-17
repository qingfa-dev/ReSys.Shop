using Shared.Application.Mediators.Queries;

namespace Module.Inventory.Features.Admin.Dashboard.Get;

public static partial class GetInventoryDashboard
{
    public sealed record Query : IQuery<Response>;
}
