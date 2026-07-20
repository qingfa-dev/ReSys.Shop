using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ReSys.ServiceDefaults;

namespace Shared.UnitTests.Infra.Aspire.ServiceDefaults;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "ServiceDefaults")]
public sealed class ExtensionsTests
{
    [Fact(DisplayName = "MapDefaultEndpoints: registers health checks in all environments")]
    public void MapDefaultEndpoints_ShouldMapHealthChecks_InAllEnvironments()
    {
        var builder = WebApplication.CreateBuilder([]);
        builder.Environment.EnvironmentName = Environments.Production;
        builder.Services.AddHealthChecks();
        var app = builder.Build();

        app.MapDefaultEndpoints();

        var dataSources = ((IEndpointRouteBuilder)app).DataSources;
        var endpoints = dataSources.SelectMany(ds => ds.Endpoints).ToList();

        var routePatterns = endpoints.OfType<RouteEndpoint>().Select(e => e.RoutePattern.RawText).ToList();
        routePatterns.Should().Contain("/health");
        routePatterns.Should().Contain("/alive");
    }
}
