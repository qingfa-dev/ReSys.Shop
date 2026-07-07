using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shared.Security.RateLimiting;

namespace Shared.UnitTests.Security.RateLimiting;

public class RateLimitTests
{
    [Fact]
    public void AddRateLimiting_RegistersPolicies()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RateLimit:Policies:Auth:PermitLimit"] = "5",
                ["RateLimit:Policies:Auth:WindowSeconds"] = "60"
            })
            .Build();
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddConfiguration(config);
        builder.AddRateLimiting();
        using var sp = builder.Services.BuildServiceProvider();
        var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RateLimiterOptions>>().Value;
        Assert.NotNull(options.GlobalLimiter);
        Assert.Equal(StatusCodes.Status429TooManyRequests, options.RejectionStatusCode);
    }
}
