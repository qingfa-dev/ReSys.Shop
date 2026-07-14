using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Shared.Security.Authentication.Contexts.Services;

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
                ["ConnectionStrings:Cache"] = "localhost:6379",

                ["Observability:UseAspireOTLPExporter"] = "false",
                ["Observability:ExposeDetailedReport"] = "true",
                ["Observability:SensitiveDataLogging"] = "false",
                ["Diagnostics:Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Warning",

                ["Caching:Enabled"] = "false",
                ["Caching:Memory:Enabled"] = "false",
                ["Caching:Distributed:Enabled"] = "false",
                ["Caching:Distributed:Required"] = "false",
                ["Caching:Hybrid:Enabled"] = "false",

                ["BackgroundJobs:Enabled"] = "false",

                ["Storage:Enabled"] = "false",
                ["Storage:MalwareScanner:Enabled"] = "false",

                ["Authentication:Jwt:Secret"] = "integration-test-secret-key-32-chars!!",
                ["Authentication:Jwt:Issuer"] = "ReSys.Shop.Test",
                ["Authentication:Jwt:Audience"] = "ReSys.Shop.Test",

                ["Authentication:Google:ClientId"] = "test-client-id",
                ["Authentication:Google:ClientSecret"] = "test-client-secret",

                ["Notification:ApplicationName"] = "ReSys Shop Test",
                ["Notification:SupportEmail"] = "support@resys.shop",
                ["Notification:SupportPhone"] = "+1-123-456-7890",
                ["Notification:ApplicationUrl"] = "http://localhost:5000",
                ["Notification:CustomerSupportLink"] = "http://localhost:5000/support",
                ["Notification:UnsubscribeUrl"] = "http://localhost:5000/unsubscribe",
                ["Notification:SurveyUrl"] = "http://localhost:5000/survey",
                ["Notification:Channels:Email:FromEmail"] = "test@resys.shop",
                ["Notification:Channels:Email:FromName"] = "test@resys.shop",
                ["Notification:Channels:Email:Enabled"] = "false",
                ["Notification:Channels:Sms:Enabled"] = "false",
                ["Notification:Channels:Sms:DefaultSenderNumber"] = "+12345678901",

                ["GuestSession:CookieName"] = "Guest",
                ["GuestSession:CookieSecurePolicy"] = "SameAsRequest",

                ["AntiForgery:HeaderName"] = "X-XSRF-TOKEN",
                ["AntiForgery:IsEnabled"] = "false",
                ["AntiForgery:Required"] = "false",
                ["AntiForgery:CookieName"] = "XSRF-TOKEN",
                ["AntiForgery:CookieSecurePolicy"] = "SameAsRequest",
                ["AntiForgery:CookieSameSite"] = "Strict",
                ["AntiForgery:CookieHttpOnly"] = "true",

                ["GatewayProviders:SettingsEncryptionKey"] = "integration-test-encryption-key-32+bytes",
                ["GatewayProviders:stripe:Enabled"] = "true",
                ["GatewayProviders:stripe:WebhookSecret"] = "whsec_integration_test_secret_32+chars"
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

            // Replace the production HTTP-context-based ICurrentUser with a
            // test stub that reads its user identifier from a process-wide
            // AsyncLocal. This allows tests to switch the "current user"
            // between MediatR invocations without going through an HTTP
            // request (e.g. for in-process concurrency tests). The stub
            // falls back to the ambient HttpContext when no AsyncLocal
            // value is set so existing HTTP-based auth tests still work.
            services.RemoveAll<ICurrentUser>();
            services.AddTransient<ICurrentUser>(sp => new TestCurrentUser(
                sp.GetRequiredService<IHttpContextAccessor>()));

            // Replace the HybridCache-dependent TokenTheftDetector with a
            // no-op stub. Caching is disabled in tests (Caching:Enabled=false)
            // so HybridCache is not registered; the real detector fails DI
            // resolution, which cascades to all consumers of IRefreshTokenService
            // (every Identity auth handler).
            services.RemoveAll<Shared.Security.Authentication.Tokens.Services.Refresh.Protections.ITokenTheftDetector>();
            services.AddScoped<Shared.Security.Authentication.Tokens.Services.Refresh.Protections.ITokenTheftDetector>(
                static _ => new Shared.Security.Authentication.Tokens.Services.Refresh.Protections.NoOpTokenTheftDetector());

            // ITokenBlacklistService is kept as-is. It depends on ICacheService
            // which is now unconditionally registered (CacheService uses
            // IServiceProvider to lazily resolve HybridCache, so it works
            // even when Caching:Enabled=false).
        });
    }

    /// <summary>
    /// In-process <see cref="ICurrentUser"/> stub used by integration tests
    /// that need to switch the current user between MediatR invocations
    /// without issuing HTTP requests. The user identifier is stored in a
    /// static <see cref="AsyncLocal{T}"/> so it flows with the async
    /// context across scope boundaries.
    /// <para>
    /// When <see cref="SetUser"/> has not been called for the current
    /// async context, the stub falls back to the ambient
    /// <see cref="HttpContext"/>'s authenticated principal so existing
    /// HTTP-based integration tests (JWT bearer, antiforgery, etc.)
    /// continue to resolve the current user from the request.
    /// </para>
    /// </summary>
    public sealed class TestCurrentUser : ICurrentUser
    {
        private static readonly AsyncLocal<Guid?> _userId = new();
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TestCurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static void SetUser(Guid? id) => _userId.Value = id;

        public static void Reset() => _userId.Value = null;

        private ClaimsPrincipal? AmbientUser => _httpContextAccessor.HttpContext?.User;
        private HttpContext? AmbientHttpContext => _httpContextAccessor.HttpContext;

        private Guid? CurrentOrAmbient()
        {
            if (_userId.Value is { } id) return id;

            ClaimsPrincipal? user = AmbientUser;
            if (user?.Identity?.IsAuthenticated != true) return null;

            string? raw = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var parsed) ? parsed : null;
        }

        public string? UserId => CurrentOrAmbient()?.ToString();

        public string? UserName
        {
            get
            {
                if (_userId.Value is not null) return $"test-user-{_userId.Value:N}".Substring(0, 16);
                return AmbientUser?.FindFirstValue(JwtRegisteredClaimNames.Name)
                       ?? AmbientUser?.FindFirstValue(ClaimTypes.Name);
            }
        }

        public string? Email
        {
            get
            {
                if (_userId.Value is not null)
                {
                    string addr = $"{_userId.Value:N}@test.local";
                    return addr.Substring(0, Math.Min(addr.Length, 32));
                }
                return AmbientUser?.FindFirstValue(JwtRegisteredClaimNames.Email)
                       ?? AmbientUser?.FindFirstValue(ClaimTypes.Email);
            }
        }

        public bool IsAuthenticated => CurrentOrAmbient() is not null;

        public string? IpAddress => AmbientHttpContext?.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

        public string? SessionId => AmbientHttpContext?.Request.Cookies["Guest"]
                                    ?? AmbientHttpContext?.Items["Guest"] as string
                                    ?? AmbientHttpContext?.Request.Headers["X-Session-Id"].FirstOrDefault();

        public string? Device => AmbientHttpContext?.Request.Headers.UserAgent.ToString() ?? "xUnit";
    }
}
