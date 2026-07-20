using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

using Shared.Security.RateLimiting;

namespace Shared.UnitTests.Security.RateLimiting;

public class RateLimitExtensionTests
{
    [Fact]
    public void AddRateLimiting_RegistersAllNamedPolicies_WithGlobalLimiter()
    {
        var builder = WebApplication.CreateBuilder([]);
        builder.AddRateLimiting();
        using var sp = builder.Services.BuildServiceProvider();
        var options = sp.GetRequiredService<IOptions<RateLimiterOptions>>().Value;

        options.RejectionStatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        options.GlobalLimiter.Should().NotBeNull();
    }

    [Fact]
    public void PolicyConstantNames_MatchExpectedValues()
    {
        RateLimitExtensions.AuthPolicy.Should().Be("auth");
        RateLimitExtensions.RegisterPolicy.Should().Be("register");
        RateLimitExtensions.ForgotPasswordPolicy.Should().Be("forgot-password");
        RateLimitExtensions.PaymentPolicy.Should().Be("payment");
        RateLimitExtensions.DefaultPolicy.Should().Be("default");
    }

    [Fact]
    public void AddRateLimiting_RegistersRateLimitSettings_AsSingleton()
    {
        var builder = WebApplication.CreateBuilder([]);
        builder.AddRateLimiting();
        using var sp = builder.Services.BuildServiceProvider();

        var setting = sp.GetRequiredService<IOptions<Shared.Security.RateLimiting.Options.RateLimitSetting>>();
        setting.Value.Should().NotBeNull();
        setting.Value.Enabled.Should().BeTrue();
    }
}
