using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Api.Tests.Infrastructure;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public ApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("https_port", "");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString,

                ["Observability:UseAspireOTLPExporter"] = "false",
                ["Observability:ExposeDetailedReport"] = "true",

                ["Caching:Enabled"] = "false",
                ["Caching:Memory:Enabled"] = "false",
                ["Caching:Distributed:Enabled"] = "false",
                ["Caching:Hybrid:Enabled"] = "false",

                ["BackgroundJobs:Enabled"] = "false",

                ["Storage:Enabled"] = "false",
                ["Storage:MalwareScanner:Enabled"] = "false",

                ["Authentication:Jwt:Secret"] = "integration-test-secret-key-32-chars!!",
                ["Authentication:Jwt:Issuer"] = "ReSys.Shop.Test",
                ["Authentication:Jwt:Audience"] = "ReSys.Shop.Test",

                ["Authentication:Google:ClientId"] = "test-client-id",
                ["Authentication:Google:ClientSecret"] = "test-client-secret",

                ["GuestSession:CookieSecurePolicy"] = "SameAsRequest",

                ["AntiForgery:HeaderName"] = "X-XSRF-TOKEN",
                ["AntiForgery:IsEnabled"] = "false",
                ["AntiForgery:Required"] = "false",
                ["AntiForgery:CookieName"] = "XSRF-TOKEN",
                ["AntiForgery:CookieSecurePolicy"] = "SameAsRequest",
                ["AntiForgery:CookieSameSite"] = "Strict",
                ["AntiForgery:CookieHttpOnly"] = "true"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();

            services.Configure<AntiforgeryOptions>(static options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
                options.Cookie.Name = "XSRF-TOKEN";
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });
        });
    }
}
