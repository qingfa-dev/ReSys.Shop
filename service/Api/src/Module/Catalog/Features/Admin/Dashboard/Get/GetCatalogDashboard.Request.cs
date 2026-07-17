using Shared.Application.Mediators.Queries;

namespace Module.Catalog.Features.Admin.Dashboard.Get;

public static partial class GetCatalogDashboard
{
    public sealed record Query : IQuery<Response>;
}
