using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shared.Operational.Notifications.Channels.Emails.Providers.Smtp;
using Shared.Security.AntiForgery.Options;

namespace Api.Tests.Scenarios.Shared;

[Trait("Category", "Integration")]
public class OptionsValidationOnStartTests
{
    [Fact(DisplayName = "ValidateOnStart: empty SMTP host fails host build")]
    public void EmptySmtpHost_FailsHostBuild()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Notification:Channels:Email:Providers:Smtp:Host"] = "",
                        ["Notification:Channels:Email:Providers:Smtp:Port"] = "1025"
                    });
                });
            });

        var act = () =>
        {
            using var scope = factory.Services.CreateScope();
            var opts = scope.ServiceProvider.GetRequiredService<IOptions<SmtpProviderSetting>>();
            _ = opts.Value;
        };

        act.Should().Throw<OptionsValidationException>()
           .WithMessage("*Smtp*Host*");
    }

    [Fact(DisplayName = "ValidateOnStart: missing anti-forgery cookie name fails host build")]
    public void EmptyAntiForgeryCookieName_FailsHostBuild()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["AntiForgery:CookieName"] = ""
                    });
                });
            });

        var act = () =>
        {
            using var scope = factory.Services.CreateScope();
            var opts = scope.ServiceProvider.GetRequiredService<IOptions<AntiForgerySetting>>();
            _ = opts.Value;
        };

        act.Should().Throw<OptionsValidationException>()
           .WithMessage("*AntiForgery*CookieName*");
    }
}
